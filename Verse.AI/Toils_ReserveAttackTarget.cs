namespace Verse.AI
{
	public static class Toils_ReserveAttackTarget
	{
		public static Toil TryReserve(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				IAttackTarget attackTarget = actor.CurJob.GetTarget(ind).Thing as IAttackTarget;
				if (attackTarget != null)
				{
					actor.Map.attackTargetReservationManager.Reserve(actor, toil.actor.CurJob, attackTarget);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.atomicWithPrevious = true;
			return toil;
		}
	}
}
