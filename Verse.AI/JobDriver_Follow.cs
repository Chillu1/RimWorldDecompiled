using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_Follow : JobDriver
	{
		private const TargetIndex FolloweeInd = TargetIndex.A;

		private const int Distance = 4;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
				if (!base.pawn.Position.InHorDistOf(pawn.Position, 4f) || !base.pawn.Position.WithinRegions(pawn.Position, base.Map, 2, TraverseParms.For(base.pawn)))
				{
					if (!base.pawn.CanReach(pawn, PathEndMode.Touch, Danger.Deadly))
					{
						EndJobWith(JobCondition.Incompletable);
					}
					else if (!base.pawn.pather.Moving || base.pawn.pather.Destination != pawn)
					{
						base.pawn.pather.StartPath(pawn, PathEndMode.Touch);
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			yield return toil;
		}

		public override bool IsContinuation(Job j)
		{
			return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}
	}
}
