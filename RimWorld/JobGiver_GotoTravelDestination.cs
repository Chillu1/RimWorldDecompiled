using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_GotoTravelDestination : ThinkNode_JobGiver
{
	protected LocomotionUrgency locomotionUrgency = LocomotionUrgency.Walk;

	protected Danger maxDanger = Danger.Some;

	protected int jobMaxDuration = 999999;

	protected bool exactCell;

	protected string ritualTagOnArrival;

	protected bool wanderOnArrival = true;

	protected JobDef jobDef;

	protected int destinationFocusIndex = 1;

	protected bool allowZeroLengthPaths;

	private IntRange WaitTicks = new IntRange(30, 80);

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_GotoTravelDestination obj = (JobGiver_GotoTravelDestination)base.DeepCopy(resolve);
		obj.locomotionUrgency = locomotionUrgency;
		obj.maxDanger = maxDanger;
		obj.jobMaxDuration = jobMaxDuration;
		obj.exactCell = exactCell;
		obj.ritualTagOnArrival = ritualTagOnArrival;
		obj.wanderOnArrival = wanderOnArrival;
		obj.jobDef = jobDef;
		obj.destinationFocusIndex = destinationFocusIndex;
		obj.allowZeroLengthPaths = allowZeroLengthPaths;
		return obj;
	}

	protected virtual LocalTargetInfo GetDestination(Pawn pawn)
	{
		switch (destinationFocusIndex)
		{
		case 1:
			return pawn.mindState.duty.focus;
		case 2:
			return pawn.mindState.duty.focusSecond;
		case 3:
			return pawn.mindState.duty.focusThird;
		default:
			Log.Error($"Invalid focus index {destinationFocusIndex}.");
			return LocalTargetInfo.Invalid;
		}
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
		Pawn pawn2 = CaravanBabyToCarry(pawn);
		if (pawn2 != null)
		{
			Job job = JobMaker.MakeJob(JobDefOf.PickupToHold, pawn2);
			job.count = 1;
			return job;
		}
		if (pawn.mindState.nextMoveOrderIsWait && !exactCell)
		{
			if (!wanderOnArrival)
			{
				return null;
			}
			return GetWaitJob();
		}
		LocalTargetInfo destination = GetDestination(pawn);
		if (!pawn.CanReach(destination, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, maxDanger)))
		{
			return null;
		}
		if (pawn.Downed)
		{
			return null;
		}
		if (exactCell && pawn.Position == destination)
		{
			if (!ritualTagOnArrival.NullOrEmpty() && pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual)
			{
				lordJob_Ritual.AddTagForPawn(pawn, ritualTagOnArrival);
			}
			if (allowZeroLengthPaths)
			{
				return GetWaitJob();
			}
			return null;
		}
		LocalTargetInfo targetA = destination;
		if (!exactCell)
		{
			targetA = CellFinder.RandomClosewalkCellNear(destination.Cell, pawn.Map, 6);
		}
		Job job2 = JobMaker.MakeJob(jobDef ?? JobDefOf.Goto, targetA);
		job2.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
		job2.expiryInterval = jobMaxDuration;
		job2.ritualTag = ritualTagOnArrival;
		return job2;
	}

	private Job GetWaitJob()
	{
		Job job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
		job.expiryInterval = WaitTicks.RandomInRange;
		return job;
	}

	private Pawn CaravanBabyToCarry(Pawn pawn)
	{
		if (!pawn.RaceProps.Humanlike || pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null))
		{
			return null;
		}
		Lord lord = pawn.GetLord();
		if (lord == null)
		{
			return null;
		}
		List<Pawn> list = (lord.LordJob as LordJob_FormAndSendCaravan)?.downedPawns;
		if (list.NullOrEmpty())
		{
			return null;
		}
		foreach (Pawn item in list)
		{
			if (item.Downed && item != pawn && item.DevelopmentalStage.Baby() && pawn.CanReserveAndReach(item, PathEndMode.Touch, Danger.Deadly) && item.Spawned)
			{
				return item;
			}
		}
		return null;
	}
}
