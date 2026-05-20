namespace Verse.AI;

public static class Toils_ReserveAttackTarget
{
	public static Toil TryReserve(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("TryReserve");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			if (actor.CurJob.GetTarget(ind).Thing is IAttackTarget target)
			{
				actor.Map.attackTargetReservationManager.Reserve(actor, toil.actor.CurJob, target);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}
}
