using System.Collections.Generic;

namespace Verse.AI;

public static class Toils_Reserve
{
	public static Toil ReserveDestination(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("ReserveDestination");
		toil.initAction = delegate
		{
			toil.actor.Map.pawnDestinationReservationManager.Reserve(toil.actor, toil.actor.CurJob, toil.actor.jobs.curJob.GetTarget(ind).Cell);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil Reserve(TargetIndex ind, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
	{
		Toil toil = ToilMaker.MakeToil("Reserve");
		toil.initAction = delegate
		{
			if (!toil.actor.Reserve(toil.actor.jobs.curJob.GetTarget(ind), toil.actor.CurJob, maxPawns, stackCount, layer, ignoreOtherReservations))
			{
				toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil ReserveDestinationOrThing(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("ReserveDestinationOrThing");
		toil.initAction = delegate
		{
			LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(ind);
			if (target.HasThing)
			{
				if (!toil.actor.Reserve(target, toil.actor.CurJob))
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			}
			else
			{
				toil.actor.Map.pawnDestinationReservationManager.Reserve(toil.actor, toil.actor.CurJob, target.Cell);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil ReserveQueue(TargetIndex ind, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null)
	{
		Toil toil = ToilMaker.MakeToil("ReserveQueue");
		toil.initAction = delegate
		{
			List<LocalTargetInfo> targetQueue = toil.actor.jobs.curJob.GetTargetQueue(ind);
			if (targetQueue != null)
			{
				for (int i = 0; i < targetQueue.Count; i++)
				{
					if (!toil.actor.Reserve(targetQueue[i], toil.actor.CurJob, maxPawns, stackCount, layer))
					{
						toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil Release(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("Release");
		toil.initAction = delegate
		{
			LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(ind);
			toil.actor.Map.reservationManager.Release(target, toil.actor, toil.actor.CurJob);
			toil.actor.jobs.ReleaseReservations(target);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil ReleaseDestination()
	{
		Toil toil = ToilMaker.MakeToil("ReleaseDestination");
		toil.initAction = delegate
		{
			toil.actor.Map.pawnDestinationReservationManager.ReleaseClaimedBy(toil.actor, toil.actor.CurJob);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	public static Toil ReleaseDestinationOrThing(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("ReleaseDestinationOrThing");
		toil.initAction = delegate
		{
			LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(ind);
			if (target.HasThing)
			{
				toil.actor.Map.reservationManager.Release(target, toil.actor, toil.actor.CurJob);
				toil.actor.jobs.ReleaseReservations(target);
			}
			else
			{
				toil.actor.Map.pawnDestinationReservationManager.ReleaseClaimedBy(toil.actor, toil.actor.CurJob);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}
}
