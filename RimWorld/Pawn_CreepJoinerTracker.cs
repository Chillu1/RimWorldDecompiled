using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class Pawn_CreepJoinerTracker : IExposable
{
	private static readonly IntRange ReleaseRejectionDelayTicks = new IntRange(60, 180);

	private const int CheckDownsideInterval = 15000;

	public CreepJoinerFormKindDef form;

	public CreepJoinerBenefitDef benefit;

	public CreepJoinerDownsideDef downside;

	public CreepJoinerAggressiveDef aggressive;

	public CreepJoinerRejectionDef rejection;

	public Pawn speaker;

	public Quest quest;

	public string spokeToSignal;

	public int timeoutAt;

	public int joinedTick;

	private int downsideTriggersAt;

	private int canTriggerDownsideAfter;

	private bool triggeredDownside;

	private bool triggeredAggressive;

	private bool triggeredRejection;

	private bool hasLeft;

	private bool entryLordEnded;

	private int triggerPrisonerRejectionAt;

	private int triggerPrisonerAggressiveAt;

	private bool duplicated;

	private BaseCreepJoinerWorker downsideWorkerInt;

	private BaseCreepJoinerWorker rejectionWorkerInt;

	private BaseCreepJoinerWorker aggressiveWorkerInt;

	private BaseCreepJoinerWorker DownsideWorker => GetWorker(downside.workerType, ref downsideWorkerInt);

	private BaseCreepJoinerWorker RejectionWorker => GetWorker(rejection.workerType, ref rejectionWorkerInt);

	private BaseCreepJoinerWorker AggressiveWorker => GetWorker(aggressive.workerType, ref aggressiveWorkerInt);

	public bool IsOnEntryLord
	{
		get
		{
			if (entryLordEnded)
			{
				return false;
			}
			Lord lord = Pawn.GetLord();
			if (lord == null)
			{
				return false;
			}
			return lord.LordJob is LordJob_CreepJoiner;
		}
	}

	public bool Disabled
	{
		get
		{
			if (!duplicated && !Pawn.everLostEgo)
			{
				return !Pawn.SpawnedOrAnyParentSpawned;
			}
			return true;
		}
	}

	public Pawn Pawn { get; }

	public bool CanTriggerAggressive
	{
		get
		{
			if (!Disabled && !triggeredAggressive)
			{
				if (AggressiveWorker != null)
				{
					return AggressiveWorker.CanDoResponse();
				}
				return true;
			}
			return false;
		}
	}

	public Pawn_CreepJoinerTracker()
	{
	}

	public Pawn_CreepJoinerTracker(Pawn pawn)
	{
		Pawn = pawn;
	}

	public void Notify_Created()
	{
		ResolveGraphics();
		DownsideWorker?.OnCreated();
		AggressiveWorker?.OnCreated();
		RejectionWorker?.OnCreated();
	}

	public void TickInterval(int delta)
	{
		if (!Disabled)
		{
			if (Pawn.IsColonist)
			{
				ColonistTickInterval(delta);
			}
			else
			{
				CheckTriggersTickInterval(delta);
			}
		}
	}

	private void CheckTriggersTickInterval(int delta)
	{
		if (triggerPrisonerAggressiveAt != 0 && GenTicks.TicksGame >= triggerPrisonerAggressiveAt && !triggeredAggressive)
		{
			DoAggressive();
		}
		else if (triggerPrisonerRejectionAt != 0 && GenTicks.TicksGame >= triggerPrisonerRejectionAt && !triggeredRejection)
		{
			DoRejection();
		}
		if (triggerPrisonerAggressiveAt == 0 && triggerPrisonerRejectionAt == 0 && downside.canOccurWhenImprisoned && Pawn.IsPrisonerOfColony && Pawn.IsHashIntervalTick(15000, delta))
		{
			CheckDownsideOccurs();
		}
	}

	private void ColonistTickInterval(int delta)
	{
		if (Pawn.IsHashIntervalTick(15000, delta))
		{
			CheckDownsideOccurs();
		}
	}

	private void CheckDownsideOccurs()
	{
		if (downside != null && (downside.repeats || !triggeredDownside) && (downside.canOccurWhenImprisoned || (!Pawn.IsPrisoner && !Pawn.IsSlave)) && (!(downside.triggerMinDays != FloatRange.Zero) || GenTicks.TicksGame >= canTriggerDownsideAfter) && (downside.canOccurWhileDowned || !Pawn.Downed) && Pawn.SpawnedOrAnyParentSpawned && (!downside.mustBeConscious || Pawn.health.capacities.CanBeAwake))
		{
			bool num = downside.triggerMtbDays != 0f && Rand.MTBEventOccurs(downside.triggerMtbDays, 60000f, 15000f);
			bool flag = downsideTriggersAt != 0 && GenTicks.TicksGame >= downsideTriggersAt;
			if (num || flag)
			{
				DoDownside();
			}
		}
	}

	public void Notify_ChangedFaction()
	{
		if (Pawn.IsColonist)
		{
			ClearLord();
			joinedTick = GenTicks.TicksGame;
			if (downside.triggersAfterDays != FloatRange.Zero)
			{
				downsideTriggersAt = GenTicks.TicksGame + (int)(downside.triggersAfterDays.RandomInRange * 60000f);
			}
			if (downside.triggerMinDays != FloatRange.Zero)
			{
				canTriggerDownsideAfter = GenTicks.TicksGame + (int)(downside.triggerMinDays.RandomInRange * 60000f);
			}
		}
	}

	public void Notify_DuplicatedFrom(Pawn _)
	{
		duplicated = true;
	}

	public void Notify_Arrested(bool succeeded)
	{
		if (!duplicated)
		{
			if (!succeeded)
			{
				DoAggressive();
			}
			else
			{
				ClearLord();
			}
		}
	}

	public void Notify_Released()
	{
		if (!duplicated)
		{
			triggerPrisonerRejectionAt = GenTicks.TicksGame + ReleaseRejectionDelayTicks.RandomInRange;
		}
	}

	public void Notify_PrisonBreakout()
	{
		if (!duplicated)
		{
			triggerPrisonerAggressiveAt = GenTicks.TicksGame + ReleaseRejectionDelayTicks.RandomInRange;
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if (!DebugSettings.ShowDevGizmos || !Pawn.IsColonist)
		{
			yield break;
		}
		if (canTriggerDownsideAfter != 0 && !triggeredDownside && GenTicks.TicksGame < canTriggerDownsideAfter && (downside.canOccurWhenImprisoned || !Pawn.IsPrisoner))
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Unlock downside trigger",
				action = delegate
				{
					canTriggerDownsideAfter = GenTicks.TicksGame;
				}
			};
		}
		else if ((downsideTriggersAt != 0 || downside.triggerMtbDays != 0f) && (!triggeredDownside || downside.repeats) && (downside.canOccurWhenImprisoned || !Pawn.IsPrisoner))
		{
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = "DEV: Trigger timed downside",
				action = DoDownside
			};
			BaseCreepJoinerWorker downsideWorker = DownsideWorker;
			if (downsideWorker != null && !downsideWorker.CanDoResponse())
			{
				command_Action.Disable("Worker is blocking this trigger from occuring.");
			}
			yield return command_Action;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Do aggressive",
			action = DoAggressive
		};
		yield return new Command_Action
		{
			defaultLabel = "DEV: Do rejection",
			action = DoRejection
		};
	}

	public string GetInspectString()
	{
		if (!DebugSettings.godMode || Disabled)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = "DEV Downside: " + downside.label;
		if (canTriggerDownsideAfter != 0 && !triggeredDownside && GenTicks.TicksGame < canTriggerDownsideAfter && (downside.canOccurWhenImprisoned || !Pawn.IsPrisoner))
		{
			text = text + " (can after: " + (canTriggerDownsideAfter - GenTicks.TicksGame).ToStringTicksToPeriod() + ")";
		}
		else if (downsideTriggersAt != 0 && !triggeredDownside && (downside.canOccurWhenImprisoned || !Pawn.IsPrisoner))
		{
			text = text + " (triggers: " + (downsideTriggersAt - GenTicks.TicksGame).ToStringTicksToPeriod() + ")";
		}
		stringBuilder.AppendLine("DEV Benefit: " + benefit?.label);
		stringBuilder.AppendLine(text);
		stringBuilder.AppendLine("DEV Rejection: " + rejection?.label);
		stringBuilder.Append("DEV Aggressive: " + aggressive?.label);
		return stringBuilder.ToString();
	}

	public void Notify_CreepJoinerSpokenTo(Pawn speaker)
	{
		if (!Disabled)
		{
			this.speaker = speaker;
			Find.SignalManager.SendSignal(new Signal(spokeToSignal));
		}
	}

	public void Notify_CreepJoinerAttacked(Pawn instigatorPawn)
	{
		if (!Disabled && instigatorPawn != null && instigatorPawn.CurJobDef != JobDefOf.SocialFight)
		{
			DoAggressive();
		}
	}

	public void Notify_CreepJoinerKilled()
	{
		if (!Disabled && AggressiveWorker.CanOccurOnDeath)
		{
			DoAggressive();
		}
	}

	public void Notify_CreepJoinerRejected()
	{
		DoRejection();
	}

	public void DoDownside()
	{
		if (Disabled || (DownsideWorker != null && !DownsideWorker.CanDoResponse()))
		{
			return;
		}
		ClearLord();
		triggeredDownside = true;
		if (downside.triggersAfterDays != FloatRange.Zero)
		{
			downsideTriggersAt = GenTicks.TicksGame + (int)(downside.triggersAfterDays.RandomInRange * 60000f);
		}
		if (downside.triggerMinDays != FloatRange.Zero)
		{
			canTriggerDownsideAfter = GenTicks.TicksGame + (int)(downside.triggerMinDays.RandomInRange * 60000f);
		}
		List<TargetInfo> list = new List<TargetInfo> { Pawn };
		List<NamedArgument> list2 = new List<NamedArgument> { Pawn.Named("PAWN") };
		foreach (HediffDef hediff2 in downside.hediffs)
		{
			if (Pawn.health.hediffSet.TryGetHediff(hediff2, out var hediff) && hediff.TryGetComp<HediffComp_ReplaceHediff>(out var comp))
			{
				comp.Trigger();
			}
		}
		DownsideWorker?.DoResponse(list, list2);
		if (downside.hasLetter)
		{
			TaggedString label = downside.letterLabel.Formatted(list2);
			TaggedString text = downside.letterDesc.Formatted(list2);
			Find.LetterStack.ReceiveLetter(label, text, downside.letterDef, list);
		}
	}

	public void DoRejection()
	{
		if (!Disabled && !triggeredRejection && !triggeredAggressive && !hasLeft && (RejectionWorker == null || RejectionWorker.CanDoResponse()))
		{
			triggeredRejection = true;
			ClearLord();
			List<TargetInfo> list = new List<TargetInfo> { Pawn };
			List<NamedArgument> list2 = new List<NamedArgument> { Pawn.Named("PAWN") };
			RejectionWorker?.DoResponse(list, list2);
			if (rejection.hasLetter)
			{
				TaggedString label = rejection.letterLabel.Formatted(list2);
				TaggedString text = rejection.letterDesc.Formatted(list2);
				Find.LetterStack.ReceiveLetter(label, text, rejection.letterDef, list);
			}
		}
	}

	public void DoAggressive()
	{
		if (CanTriggerAggressive)
		{
			triggeredAggressive = true;
			ClearLord();
			List<TargetInfo> list = new List<TargetInfo> { Pawn };
			List<NamedArgument> list2 = new List<NamedArgument> { Pawn.Named("PAWN") };
			AggressiveWorker?.DoResponse(list, list2);
			if (aggressive.hasMessage)
			{
				Messages.Message(aggressive.message.Formatted(list2), list, MessageTypeDefOf.NegativeEvent);
			}
			if (aggressive.hasLetter)
			{
				TaggedString label = aggressive.letterLabel.Formatted(list2);
				TaggedString text = aggressive.letterDesc.Formatted(list2);
				Find.LetterStack.ReceiveLetter(label, text, aggressive.letterDef, list);
			}
		}
	}

	public void DoLeave()
	{
		if (!Disabled && !hasLeft)
		{
			ClearLord();
			if (Pawn.Faction != null && Pawn.Faction.IsPlayer)
			{
				Pawn.SetFaction(null);
			}
			LordMaker.MakeNewLord(Pawn.Faction, new LordJob_ExitMapBest(LocomotionUrgency.Jog), Pawn.Map).AddPawn(Pawn);
			hasLeft = true;
		}
	}

	public bool DoSurgicalInspection(Pawn surgeon, StringBuilder sb)
	{
		bool result = false;
		if (!benefit.surgicalInspectionLetterExtra.NullOrEmpty())
		{
			sb.Append("\n\n" + benefit.surgicalInspectionLetterExtra.Formatted(Pawn.Named("PAWN"), surgeon.Named("SURGEON")));
			result = true;
		}
		if (!downside.surgicalInspectionLetterExtra.NullOrEmpty())
		{
			sb.Append("\n\n" + downside.surgicalInspectionLetterExtra.Formatted(Pawn.Named("PAWN"), surgeon.Named("SURGEON")));
			result = true;
		}
		if (!aggressive.surgicalInspectionLetterExtra.NullOrEmpty())
		{
			sb.Append("\n\n" + aggressive.surgicalInspectionLetterExtra.Formatted(Pawn.Named("PAWN"), surgeon.Named("SURGEON")));
			result = true;
		}
		if (!rejection.surgicalInspectionLetterExtra.NullOrEmpty())
		{
			sb.Append("\n\n" + rejection.surgicalInspectionLetterExtra.Formatted(Pawn.Named("PAWN"), surgeon.Named("SURGEON")));
			result = true;
		}
		return result;
	}

	public IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		if (!Disabled && IsOnEntryLord)
		{
			yield return (!selPawn.CanReach(Pawn, PathEndMode.OnCell, Danger.Deadly)) ? new FloatMenuOption("CannotTalkTo".Translate(Pawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null) : (selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TalkTo".Translate(Pawn), delegate
			{
				Job job = JobMaker.MakeJob(JobDefOf.TalkCreepJoiner, Pawn);
				job.playerForced = true;
				selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}), selPawn, Pawn) : new FloatMenuOption("CannotTalkTo".Translate(Pawn) + ": " + "Incapable".Translate().CapitalizeFirst(), null));
		}
	}

	private void ResolveGraphics()
	{
		if (!form.forcedHeadTypes.NullOrEmpty())
		{
			Pawn.story.TryGetRandomHeadFromSet(form.forcedHeadTypes);
		}
		if (form.hairTagFilter != null)
		{
			Pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(Pawn);
		}
		if (form.beardTagFilter != null)
		{
			Pawn.style.beardDef = PawnStyleItemChooser.RandomBeardFor(Pawn);
		}
		if (form.hairColorOverride.HasValue)
		{
			Pawn.story.HairColor = form.hairColorOverride.Value;
		}
		Pawn.Drawer.renderer.SetAllGraphicsDirty();
	}

	private void ClearLord()
	{
		if (IsOnEntryLord)
		{
			entryLordEnded = true;
		}
		Pawn.GetLord()?.Notify_PawnLost(Pawn, PawnLostCondition.Undefined);
	}

	private T GetWorker<T>(Type type, ref T worker) where T : BaseCreepJoinerWorker
	{
		if (worker != null)
		{
			return worker;
		}
		if (type == null)
		{
			return null;
		}
		worker = (T)Activator.CreateInstance(type);
		worker.Tracker = this;
		return worker;
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref form, "form");
		Scribe_Defs.Look(ref benefit, "benefit");
		Scribe_Defs.Look(ref downside, "downside");
		Scribe_Defs.Look(ref rejection, "rejection");
		Scribe_Defs.Look(ref aggressive, "aggressive");
		Scribe_Values.Look(ref timeoutAt, "timeoutAt", 0);
		Scribe_Values.Look(ref duplicated, "duplicated", defaultValue: false);
		Scribe_Values.Look(ref joinedTick, "joinedTick", 0);
		Scribe_Values.Look(ref spokeToSignal, "spokeToSignal");
		Scribe_Values.Look(ref triggeredDownside, "triggeredDownside", defaultValue: false);
		Scribe_Values.Look(ref triggeredAggressive, "triggeredAggressive", defaultValue: false);
		Scribe_Values.Look(ref triggeredRejection, "triggeredRejection", defaultValue: false);
		Scribe_Values.Look(ref hasLeft, "hasLeft", defaultValue: false);
		Scribe_Values.Look(ref canTriggerDownsideAfter, "canTriggerDownsideAfter", 0);
		Scribe_Values.Look(ref downsideTriggersAt, "downsideTriggersAt", 0);
		Scribe_Values.Look(ref entryLordEnded, "entryLordEnded", defaultValue: false);
		Scribe_References.Look(ref quest, "quest");
		Scribe_References.Look(ref speaker, "speaker");
	}
}
