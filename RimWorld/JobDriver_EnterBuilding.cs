using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_EnterBuilding : JobDriver
	{
		public const int EnterDelay = 60;

		private Building_Enterable Building => (Building_Enterable)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => !Building.CanAcceptPawn(pawn));
			yield return Toils_General.Do(delegate
			{
				Building.SelectedPawn = pawn;
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_General.WaitWith(TargetIndex.A, 60, useProgressBar: true);
			yield return Toils_General.Do(delegate
			{
				Building.TryAcceptPawn(pawn);
			});
		}
	}
}
