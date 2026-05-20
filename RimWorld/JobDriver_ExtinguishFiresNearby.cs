using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ExtinguishFiresNearby : JobDriver
{
	private int ticksSpentExtinguishing;

	private readonly int maxDurationTicks = 600f.SecondsToTicks();

	private const TargetIndex FireInd = TargetIndex.A;

	private const float MaxDurationSeconds = 600f;

	protected Fire TargetFire => (Fire)job.targetA.Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		AddEndCondition(() => (ticksSpentExtinguishing < maxDurationTicks) ? JobCondition.Ongoing : JobCondition.Succeeded);
		Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A, (Thing thing) => !thing.Destroyed);
		Toil findClosestFire = ToilMaker.MakeToil("MakeNewToils");
		findClosestFire.tickIntervalAction = delegate
		{
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn), 9999f, (Thing f) => !job.GetTargetQueue(TargetIndex.A).Contains(f) && pawn.CanReserve(f), null, 0, 13);
			if (thing != null)
			{
				job.AddQueuedTarget(TargetIndex.A, thing);
				job.GetTargetQueue(TargetIndex.A).RemoveWhere((LocalTargetInfo f) => f == null || f.ThingDestroyed);
				job.GetTargetQueue(TargetIndex.A).Sort(delegate(LocalTargetInfo f1, LocalTargetInfo f2)
				{
					Fire fire = (Fire)(Thing)f1;
					Fire fire2 = (Fire)(Thing)f2;
					int num = pawn.Position.DistanceToSquared(fire.Position);
					int num2 = pawn.Position.DistanceToSquared(fire2.Position);
					if (num == num2)
					{
						return 0;
					}
					if (num < num2)
					{
						return -1;
					}
					return (num > num2) ? 1 : 0;
				});
			}
			ReadyForNextToil();
		};
		findClosestFire.defaultCompleteMode = ToilCompleteMode.Never;
		yield return initExtractTargetFromQueue;
		yield return Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A, (Thing thing) => !thing.Destroyed);
		yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
		yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
		Toil beat = ToilMaker.MakeToil("MakeNewToils");
		Toil approach = ToilMaker.MakeToil("MakeNewToils");
		approach.initAction = delegate
		{
			if (base.Map.reservationManager.CanReserve(pawn, TargetFire))
			{
				pawn.Reserve(TargetFire, job);
			}
			pawn.pather.StartPath(TargetFire, PathEndMode.Touch);
		};
		approach.tickIntervalAction = delegate
		{
			if (pawn.pather.Moving && pawn.pather.nextCell != TargetFire.Position)
			{
				StartBeatingFireIfAnyAt(pawn.pather.nextCell, beat);
			}
			if (pawn.Position != TargetFire.Position)
			{
				StartBeatingFireIfAnyAt(pawn.Position, beat);
			}
		};
		approach.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, findClosestFire);
		approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		approach.atomicWithPrevious = true;
		yield return approach;
		beat.tickAction = delegate
		{
			ticksSpentExtinguishing++;
			if (!pawn.CanReachImmediate(TargetFire, PathEndMode.Touch))
			{
				JumpToToil(approach);
			}
			else if (!(pawn.Position != TargetFire.Position) || !StartBeatingFireIfAnyAt(pawn.Position, beat))
			{
				pawn.natives.TryBeatFire(TargetFire);
				if (TargetFire.Destroyed)
				{
					pawn.records.Increment(RecordDefOf.FiresExtinguished);
					ReadyForNextToil();
				}
			}
		};
		beat.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, findClosestFire);
		beat.defaultCompleteMode = ToilCompleteMode.Never;
		yield return beat;
		yield return findClosestFire;
		yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
		yield return Toils_Jump.Jump(initExtractTargetFromQueue);
	}

	private bool StartBeatingFireIfAnyAt(IntVec3 cell, Toil nextToil)
	{
		List<Thing> thingList = cell.GetThingList(base.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Fire { parent: null } fire)
			{
				job.targetA = fire;
				pawn.pather.StopDead();
				JumpToToil(nextToil);
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksSpentExtinguishing, "ticksSpentExtinguishing", 0);
	}
}
