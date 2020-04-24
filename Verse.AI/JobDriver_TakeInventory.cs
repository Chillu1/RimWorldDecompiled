using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_TakeInventory : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				pawn.pather.StartPath(base.TargetThingA, PathEndMode.ClosestTouch);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return toil;
			yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
		}
	}
}
