using System;
using RimWorld;
using RimWorld.Planet;

namespace Verse.AI
{
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
				if (Current.ProgramState != 0 && pawn.Spawned)
				{
					pawn.Map.attackTargetsCache.UpdateTarget(pawn);
				}
			}
		}

		public void Reset()
		{
			ClearMentalStateDirect();
		}

		public void MentalStateHandlerTick()
		{
			if (curStateInt != null)
			{
				if (pawn.Downed && curStateInt.def.recoverFromDowned)
				{
					Log.Error("In mental state while downed, but not allowed: " + pawn);
					CurState.RecoverFromState();
				}
				else
				{
					curStateInt.MentalStateTick();
				}
			}
		}

		public bool TryStartMentalState(MentalStateDef stateDef, string reason = null, bool forceWake = false, bool causedByMood = false, Pawn otherPawn = null, bool transitionSilently = false)
		{
			if ((!pawn.Spawned && !pawn.IsCaravanMember()) || CurStateDef == stateDef || pawn.Downed || (!forceWake && !pawn.Awake()))
			{
				return false;
			}
			if (TutorSystem.TutorialMode && pawn.Faction == Faction.OfPlayer)
			{
				return false;
			}
			if (!stateDef.Worker.StateCanOccur(pawn))
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
			if (otherPawn != null)
			{
				((MentalState_SocialFighting)mentalState).otherPawn = otherPawn;
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
			curStateInt = mentalState;
			if (pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			if (stateDef != null && stateDef.IsAggro && pawn.caller != null)
			{
				pawn.caller.Notify_InAggroMentalState();
			}
			if (curStateInt != null)
			{
				curStateInt.PostStart(reason);
			}
			if (pawn.CurJob != null)
			{
				pawn.jobs.StopAll();
			}
			if (pawn.Spawned)
			{
				pawn.Map.attackTargetsCache.UpdateTarget(pawn);
			}
			if (pawn.Spawned && forceWake && !pawn.Awake())
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			if (!transitionSilently && PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				string text = mentalState.GetBeginLetterText();
				if (!text.NullOrEmpty())
				{
					string str = (stateDef.beginLetterLabel ?? ((string)stateDef.LabelCap)).CapitalizeFirst() + ": " + pawn.LabelShortCap;
					if (!reason.NullOrEmpty())
					{
						text = text + "\n\n" + reason;
					}
					Find.LetterStack.ReceiveLetter(str, text, stateDef.beginLetterDef, pawn);
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
					TryStartMentalState(MentalStateDefOf.PanicFlee);
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
}
