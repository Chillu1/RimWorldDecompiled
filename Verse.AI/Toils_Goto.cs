using RimWorld;

namespace Verse.AI
{
	public static class Toils_Goto
	{
		public static Toil Goto(TargetIndex ind, PathEndMode peMode)
		{
			return GotoThing(ind, peMode);
		}

		public static Toil GotoThing(TargetIndex ind, PathEndMode peMode)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				actor.pather.StartPath(actor.jobs.curJob.GetTarget(ind), peMode);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.FailOnDespawnedOrNull(ind);
			return toil;
		}

		public static Toil GotoThing(TargetIndex ind, IntVec3 exactCell)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.pather.StartPath(exactCell, PathEndMode.OnCell);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.FailOnDespawnedOrNull(ind);
			return toil;
		}

		public static Toil GotoCell(TargetIndex ind, PathEndMode peMode)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				actor.pather.StartPath(actor.jobs.curJob.GetTarget(ind), peMode);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static Toil GotoCell(IntVec3 cell, PathEndMode peMode)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				toil.actor.pather.StartPath(cell, peMode);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static Toil MoveOffTargetBlueprint(TargetIndex targetInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Thing thing = actor.jobs.curJob.GetTarget(targetInd).Thing as Blueprint;
				IntVec3 result;
				if (thing == null || !actor.Position.IsInside(thing))
				{
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else if (RCellFinder.TryFindGoodAdjacentSpotToTouch(actor, thing, out result))
				{
					actor.pather.StartPath(result, PathEndMode.OnCell);
				}
				else
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}
	}
}
