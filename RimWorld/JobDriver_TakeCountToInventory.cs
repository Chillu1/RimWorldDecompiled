using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_TakeCountToInventory : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 10, job.count);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
		}
	}
}
