using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Nuzzle : JobDriver
	{
		private const int NuzzleDuration = 100;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).socialMode = RandomSocialMode.Off;
			Toils_General.WaitWith(TargetIndex.A, 100, useProgressBar: false, maintainPosture: true).socialMode = RandomSocialMode.Off;
			yield return Toils_General.Do(delegate
			{
				Pawn recipient = (Pawn)pawn.CurJob.targetA.Thing;
				pawn.interactions.TryInteractWith(recipient, InteractionDefOf.Nuzzle);
			});
		}
	}
}
