using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_SingleInteraction : JobDriver
	{
		private const TargetIndex OtherPawnInd = TargetIndex.A;

		private Pawn OtherPawn => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			Toil toil = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			toil.socialMode = RandomSocialMode.Off;
			yield return toil;
			yield return Toils_Interpersonal.Interact(TargetIndex.A, job.interaction);
		}
	}
}
