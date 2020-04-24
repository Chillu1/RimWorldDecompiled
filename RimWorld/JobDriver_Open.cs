using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Open : JobDriver
	{
		public const int OpenTicks = 300;

		private IOpenable Openable => (IOpenable)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (!Openable.CanOpen)
				{
					base.Map.designationManager.DesignationOn(job.targetA.Thing, DesignationDefOf.Open)?.Delete();
				}
			};
			yield return toil.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.Open).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_General.Wait(300).WithProgressBarToilDelay(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A)
				.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_General.Open(TargetIndex.A);
		}
	}
}
