using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Maintain : JobDriver
	{
		private const int MaintainTicks = 180;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil toil = Toils_General.Wait(180);
			toil.WithProgressBarToilDelay(TargetIndex.A);
			toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return toil;
			Toil maintain = new Toil();
			maintain.initAction = delegate
			{
				maintain.actor.CurJob.targetA.Thing.TryGetComp<CompMaintainable>().Maintained();
			};
			maintain.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return maintain;
		}
	}
}
