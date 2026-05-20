using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Open : JobDriver
	{
		private IOpenable Openable => (IOpenable)job.targetA.Thing;

		private Thing Target => job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				if (!Openable.CanOpen)
				{
					base.Map.designationManager.DesignationOn(job.targetA.Thing, DesignationDefOf.Open)?.Delete();
				}
			};
			yield return toil.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnThingMissingDesignation(TargetIndex.A, DesignationDefOf.Open).FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil2 = Toils_General.Wait(Openable.OpenTicks, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A)
				.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			if (Target.def.building != null && Target.def.building.openingStartedSound != null)
			{
				toil2.PlaySoundAtStart(Target.def.building.openingStartedSound);
			}
			yield return toil2;
			yield return Toils_General.Open(TargetIndex.A);
		}
	}
}
