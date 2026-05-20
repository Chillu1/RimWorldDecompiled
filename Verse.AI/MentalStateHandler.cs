using System;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace Verse.AI;

public class MentalStateHandler : IExposable
{
	private Pawn pawn;

	private MentalState curStateInt;

	public bool neverFleeIndividual;

	public bool InMentalState => curStateInt != null;

	public MentalStateDef CurStateDef
	{
		get
		{
			if (curStateInt == null)
			{
				return null;
			}
			return curStateInt.def;
		}
	}

	public MentalState CurState => curStateInt;

	public MentalStateHandler()
	{
	}

	public MentalStateHandler(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref curStateInt, "curState");
		Scribe_Values.Look(ref neverFleeIndividual, "neverFleeIndividual", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (curStateInt != null)
			{
				curStateInt.pawn = pawn;
			}
			if (Current.ProgramState != ProgramState.Entry && pawn.Spawned)
			{
				pawn.Map.attackTargetsCache.UpdateTarget(pawn);
			}
		}
	}

	public void Reset()
	{
		ClearMentalStateDirect();
	}

	public void MentalStateHandlerTickInterval(int delta)
	{
		if (curStateInt != null)
		{
			if (pawn.Downed && curStateInt.def.recoverFromDowned)
			{
				Log.Error("In mental state while downed or deathresting, but not allowed: " + pawn);
				CurState.RecoverFromState();
			}
			else
			{
				curStateInt.MentalStateTick(delta);
			}
		}
	}

	public bool MentalBreaksBlocked()
	{
		foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
		{
			if (hediff.CurStage != null && hediff.CurStage.blocksMentalBreaks)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryStartMentalState(MentalStateDef stateDef, string reason = null, bool forced = false, bool forceWake = false, bool causedByMood = false, Pawn otherPawn = null, bool transitionSilently = false, bool causedByDamage = false, bool causedByPsycast = false)
	{
		if (!forced && MentalBreaksBlocked())
		{
			return false;
		}
		if (pawn.IsMutant && pawn.mutant.Def.preventsMentalBreaks)
		{
			return false;
		}
		if (CurStateDef == stateDef || (!forceWake && !pawn.Awake()))
		{
			return false;
		}
		if (TutorSystem.TutorialMode && pawn.Faction == Faction.OfPlayer)
		{
			return false;
		}
		if (!forced && !stateDef.Worker.StateCanOccur(pawn))
		{
			return false;
		}
		if (curStateInt != null && !transitionSilently)
		{
			curStateInt.RecoverFromState();
		}
		MentalState mentalState = (MentalState)Activator.CreateInstance(stateDef.stateClass);
		mentalState.pawn = pawn;
		mentalState.def = stateDef;
		mentalState.causedByMood = causedByMood;
		mentalState.causedByDamage = causedByDamage;
		mentalState.causedByPsycast = causedByPsycast;
		mentalState.causedByPawn = otherPawn;
		if (mentalState is MentalState_SocialFighting mentalState_SocialFighting)
		{
			mentalState_SocialFighting.otherPawn = otherPawn;
		}
		mentalState.PreStart();
		if (!transitionSilently)
		{
			if ((pawn.IsColonist || pawn.HostFaction == Faction.OfPlayer) && stateDef.tale != null)
			{
				TaleRecorder.RecordTale(stateDef.tale, pawn);
			}
			if (stateDef.IsExtreme && pawn.IsPlayerControlledCaravanMember())
			{
				Messages.Message("MessageCaravanMemberHasExtremeMentalBreak".Translate(), pawn.GetCaravan(), MessageTypeDefOf.ThreatSmall);
			}
			pawn.records.Increment(RecordDefOf.TimesInMentalState);
		}
		if (pawn.Drafted)
		{
			pawn.drafter.Drafted = false;
		}
		if (pawn.mechanitor != null)
		{
			pawn.mechanitor.UndraftAllMechs();
		}
		curStateInt = mentalState;
		if (pawn.needs.mood != null)
		{
			pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
		}
		if (stateDef != null && stateDef.IsAggro && pawn.caller != null)
		{
			pawn.caller.Notify_InAggroMentalState();
		}
		Lord lord = pawn.GetLord();
		lord?.Notify_InMentalState(pawn, stateDef);
		if (curStateInt != null)
		{
			curStateInt.PostStart(reason);
		}
		Thing resultingThing;
		if (stateDef.stopsJobs && pawn.CurJob != null)
		{
			pawn.jobs.StopAll();
			if (pawn.IsCarrying())
			{
				pawn.carryTracker.TryDropCarriedThing(pawn.PositionHeld, ThingPlaceMode.Near, out resultingThing);
			}
		}
		if (pawn.Spawned)
		{
			pawn.Map.attackTargetsCache.UpdateTarget(pawn);
		}
		if (pawn.Spawned && forceWake)
		{
			lord?.Notify_DormancyWakeup();
			if (!pawn.Awake())
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
		if (pawn.ParentHolder is CompTransporter compTransporter)
		{
			compTransporter.innerContainer.TryDrop(pawn, ThingPlaceMode.Near, out resultingThing);
		}
		if (!transitionSilently && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			TaggedString beginLetterText = mentalState.GetBeginLetterText();
			if (!beginLetterText.NullOrEmpty())
			{
				string text = (stateDef.beginLetterLabel ?? ((string)stateDef.LabelCap)).CapitalizeFirst() + ": " + pawn.LabelShortCap;
				if (!reason.NullOrEmpty())
				{
					beginLetterText += "\n\n" + reason;
				}
				Find.LetterStack.ReceiveLetter(text, beginLetterText, stateDef.beginLetterDef, pawn);
			}
		}
		return true;
	}

	public void Notify_DamageTaken(DamageInfo dinfo)
	{
		if (!neverFleeIndividual && pawn.Spawned && pawn.MentalStateDef == null && !pawn.Downed && dinfo.Def.ExternalViolenceFor(pawn) && pawn.RaceProps.Humanlike && pawn.mindState.canFleeIndividual)
		{
			float lerpPct = (float)(pawn.HashOffset() % 100) / 100f;
			float num = pawn.kindDef.fleeHealthThresholdRange.LerpThroughRange(lerpPct);
			if (pawn.health.summaryHealth.SummaryHealthPercent < num && pawn.Faction != Faction.OfPlayer && pawn.HostFaction == null)
			{
				TryStartMentalState(MentalStateDefOf.PanicFlee, null, forced: false, forceWake: false, causedByMood: false, null, transitionSilently: false, causedByDamage: true);
			}
		}
	}

	internal void ClearMentalStateDirect()
	{
		if (curStateInt != null)
		{
			curStateInt = null;
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "ExitMentalState", pawn.Named("SUBJECT"));
			if (pawn.Spawned)
			{
				pawn.Map.attackTargetsCache.UpdateTarget(pawn);
			}
		}
	}
}
