using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Spectate : JobDriver
	{
		private const TargetIndex MySpotOrChairInd = TargetIndex.A;

		private const TargetIndex WatchTargetInd = TargetIndex.B;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (job.GetTarget(TargetIndex.A).HasThing)
			{
				this.EndOnDespawnedOrNull(TargetIndex.A);
			}
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				pawn.rotationTracker.FaceCell(job.GetTarget(TargetIndex.B).Cell);
				pawn.GainComfortFromCellIfPossible();
				if (pawn.IsHashIntervalTick(100))
				{
					pawn.jobs.CheckForJobOverride();
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.handlingFacing = true;
			yield return toil;
		}
	}
}
