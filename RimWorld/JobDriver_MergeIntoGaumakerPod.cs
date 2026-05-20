using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_MergeIntoGaumakerPod : JobDriver
	{
		private const int WaitTicks = 120;

		private CompTreeConnection TreeComp => job.targetA.Thing.TryGetComp<CompTreeConnection>();

		private CompGaumakerPod GaumakerPod => job.targetB.Thing.TryGetComp<CompGaumakerPod>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnDespawnedOrNull(TargetIndex.B);
			this.FailOn(() => GaumakerPod.Full);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return Toils_General.WaitWith(TargetIndex.B, 120, useProgressBar: true);
			yield return Toils_General.Do(delegate
			{
				GaumakerPod.TryAcceptPawn(pawn);
			});
		}
	}
}
