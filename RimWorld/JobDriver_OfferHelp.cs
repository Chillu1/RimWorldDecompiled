using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_OfferHelp : JobDriver
	{
		private const TargetIndex OtherPawnInd = TargetIndex.A;

		public Pawn OtherPawn => (Pawn)pawn.CurJob.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => !OtherPawn.mindState.WillJoinColonyIfRescued);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.DoAtomic(delegate
			{
				OtherPawn.mindState.JoinColonyBecauseRescuedBy(pawn);
			});
		}
	}
}
