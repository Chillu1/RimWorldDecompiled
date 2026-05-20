using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Romance : JobDriver
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
			Toil toil = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			toil.socialMode = RandomSocialMode.Off;
			yield return toil;
			Toil finalGoto = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return Toils_Jump.JumpIf(finalGoto, () => !OtherPawn.Awake());
			Toil toil2 = Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			toil2.socialMode = RandomSocialMode.Off;
			yield return toil2;
			finalGoto.socialMode = RandomSocialMode.Off;
			yield return finalGoto;
			yield return Toils_General.Do(delegate
			{
				if (!OtherPawn.Awake())
				{
					OtherPawn.jobs.SuspendCurrentJob(JobCondition.InterruptForced);
					if (!pawn.interactions.CanInteractNowWith(OtherPawn, InteractionDefOf.RomanceAttempt))
					{
						Messages.Message("RomanceFailedUnexpected".Translate(pawn, OtherPawn), MessageTypeDefOf.NegativeEvent, historical: false);
					}
				}
			});
			yield return Toils_Interpersonal.Interact(TargetIndex.A, job.interaction);
		}
	}
}
