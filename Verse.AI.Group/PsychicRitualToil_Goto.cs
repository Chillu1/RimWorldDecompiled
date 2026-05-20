using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public class PsychicRitualToil_Goto : PsychicRitualToil
{
	protected const string FinalSyncTag = "final";

	protected const string InitialSyncTag = "initial";

	protected const string GotoTag = "goto";

	public bool allowWanderWhileWaiting;

	protected Dictionary<PsychicRitualRoleDef, List<IntVec3>> rolePositions;

	protected JobSyncTracker initialGoto;

	protected JobSyncTracker finalGoto;

	private bool finalGatherPhaseStarted;

	private int lastUpdateDutyTick;

	private int updatesInRow;

	private static List<Pawn> tmpControlledPawns = new List<Pawn>(8);

	public bool AnyPawnsArrived => finalGoto.AnyPawnsDone;

	public bool FinalGatherPhase
	{
		get
		{
			if (allowWanderWhileWaiting)
			{
				return initialGoto.AllPawnsDone;
			}
			return true;
		}
	}

	public virtual IEnumerable<Pawn> BlockingPawns
	{
		get
		{
			if (FinalGatherPhase)
			{
				return finalGoto.BlockingPawns;
			}
			return initialGoto.BlockingPawns;
		}
	}

	protected PsychicRitualToil_Goto()
	{
	}

	public PsychicRitualToil_Goto(IReadOnlyDictionary<PsychicRitualRoleDef, List<IntVec3>> rolePositions)
	{
		this.rolePositions = new Dictionary<PsychicRitualRoleDef, List<IntVec3>>(rolePositions.Count);
		foreach (var (key, collection) in rolePositions)
		{
			this.rolePositions[key] = new List<IntVec3>(collection);
		}
	}

	public virtual List<Pawn> ControlledPawns(PsychicRitual psychicRitual)
	{
		tmpControlledPawns.Clear();
		foreach (PsychicRitualRoleDef key in rolePositions.Keys)
		{
			if (key.psychicRitualWaitsForArrival)
			{
				tmpControlledPawns.AddRange(psychicRitual.assignments.AssignedPawns(key));
			}
		}
		return tmpControlledPawns;
	}

	public virtual bool WaitingOnPawn(Pawn pawn)
	{
		if (FinalGatherPhase)
		{
			return finalGoto.WaitingOnPawn(pawn);
		}
		return initialGoto.WaitingOnPawn(pawn);
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		List<Pawn> pawns = ControlledPawns(psychicRitual);
		initialGoto = new JobSyncTracker(pawns, this, "initial", "goto");
		finalGoto = new JobSyncTracker(pawns, this, "final", "goto");
		finalGatherPhaseStarted = !allowWanderWhileWaiting;
	}

	public override void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		bool finalGatherPhase = FinalGatherPhase;
		foreach (KeyValuePair<PsychicRitualRoleDef, List<IntVec3>> rolePosition in rolePositions)
		{
			rolePosition.Deconstruct(out var key, out var value);
			PsychicRitualRoleDef role = key;
			List<IntVec3> list = value;
			int num = 0;
			foreach (Pawn item in psychicRitual.assignments.AssignedPawns(role))
			{
				IntVec3 intVec = list[num++];
				DutyDef def = ((!finalGatherPhase && !initialGoto.WaitingOnPawn(item)) ? DutyDefOf.WanderClose : (item.IsPrisonerOfColony ? DutyDefOf.Idle : DutyDefOf.Goto));
				string text = (finalGatherPhase ? "final" : "initial");
				LocalTargetInfo? focus = intVec;
				string tag = text;
				SetPawnDuty(item, psychicRitual, parent, def, focus, null, null, tag);
			}
		}
	}

	public override bool Tick(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		foreach (Pawn item in ControlledPawns(psychicRitual))
		{
			if (item.mindState?.duty == null)
			{
				updatesInRow = ((lastUpdateDutyTick == GenTicks.TicksGame - 1) ? updatesInRow++ : 0);
				lastUpdateDutyTick = GenTicks.TicksGame;
				if (updatesInRow == 10)
				{
					Debug.LogWarning("UpdateAllDuties called 10 times in 10 ticks, ensure controlled pawns are being assigned valid duties.");
				}
				UpdateAllDuties(psychicRitual, parent);
				break;
			}
		}
		foreach (Pawn item2 in ControlledPawns(psychicRitual))
		{
			if (item2.IsPrisonerOfColony)
			{
				IntVec3 position = item2.Position;
				IntVec3? obj = item2.mindState?.duty?.focus.Cell;
				if (position == obj)
				{
					ForceFinished(item2);
				}
			}
			if (item2.Downed && item2.Spawned)
			{
				IntVec3 position = item2.Position;
				IntVec3? obj2 = item2.mindState?.duty?.focus.Cell;
				if (!(position != obj2))
				{
					ForceFinished(item2);
				}
			}
		}
		if (FinalGatherPhase && !finalGatherPhaseStarted)
		{
			finalGatherPhaseStarted = true;
			UpdateAllDuties(psychicRitual, parent);
			foreach (Pawn item3 in ControlledPawns(psychicRitual))
			{
				item3.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
		return finalGoto.AllPawnsDone;
	}

	private void ForceFinished(Pawn pawn)
	{
		if (FinalGatherPhase)
		{
			finalGoto.ForcePawnFinished(pawn);
		}
		else
		{
			initialGoto.ForcePawnFinished(pawn);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref finalGatherPhaseStarted, "finalGatherPhaseStarted", defaultValue: false);
		Scribe_Deep.Look(ref initialGoto, "initialGoto");
		Scribe_Deep.Look(ref finalGoto, "finalGoto");
		Scribe_Values.Look(ref allowWanderWhileWaiting, "allowWanderWhileWaiting", defaultValue: false);
		Scribe_Collections.Look(ref rolePositions, "rolePositions", LookMode.Def, LookMode.Value);
	}

	public override void Notify_PawnJobDone(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, Job job, JobCondition condition)
	{
		initialGoto.Notify_PawnJobDone(pawn, job, condition);
		finalGoto.Notify_PawnJobDone(pawn, job, condition);
		LocalTargetInfo value = (LocalTargetInfo)psychicRitual.assignments.Target;
		if (FinalGatherPhase)
		{
			if (!finalGoto.WaitingOnPawn(pawn))
			{
				SetPawnDuty(pawn, psychicRitual, parent, DutyDefOf.WaitForRitualParticipants, value, pawn.PositionHeld);
			}
		}
		else if (!initialGoto.WaitingOnPawn(pawn))
		{
			SetPawnDuty(pawn, psychicRitual, parent, DutyDefOf.WaitForRitualParticipants, value, pawn.PositionHeld);
		}
	}

	public override void Notify_PawnLost(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn, PawnLostCondition cond)
	{
		initialGoto.Notify_PawnLost(pawn);
		finalGoto.Notify_PawnLost(pawn);
	}

	public override ThinkResult Notify_DutyResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		if (result.Job != null)
		{
			return result;
		}
		PsychicRitualRoleDef psychicRitualRoleDef = psychicRitual.assignments.RoleForPawn(pawn);
		if (psychicRitualRoleDef == null)
		{
			return result;
		}
		if (psychicRitualRoleDef.ConditionAllowed(PsychicRitualRoleDef.Condition.NoPath))
		{
			return result;
		}
		if (psychicRitualRoleDef.CanReach(pawn, psychicRitual.assignments.Target))
		{
			return result;
		}
		TaggedString reason = PsychicRitualRoleDef.ConditionToReason(pawn, PsychicRitualRoleDef.Condition.NoPath);
		psychicRitual.LeaveOrCancelPsychicRitual(psychicRitualRoleDef, pawn, reason);
		return result;
	}

	public override string GetJobReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		bool finalGatherPhase = FinalGatherPhase;
		if (!finalGatherPhase && initialGoto.WaitingOnPawn(pawn))
		{
			return "PsychicRitualToil_Goto_JobReport_Moving".Translate();
		}
		if (finalGatherPhase && finalGoto.WaitingOnPawn(pawn))
		{
			return "PsychicRitualToil_Goto_JobReport_Moving".Translate();
		}
		return "PsychicRitualToil_Goto_JobReport_Waiting".Translate();
	}
}
