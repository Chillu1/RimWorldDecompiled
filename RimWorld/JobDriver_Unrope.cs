using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Unrope : JobDriver
	{
		private const TargetIndex AnimalInd = TargetIndex.A;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A);
			yield return Toils_Rope.GotoRopeAttachmentInteractionCell(TargetIndex.A);
			yield return Toils_Rope.UnropeFromSpot(TargetIndex.A);
		}
	}
}
