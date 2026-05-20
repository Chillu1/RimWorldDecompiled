using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_HaulMechToCharger : JobDriver
	{
		private const TargetIndex MechInd = TargetIndex.A;

		private const TargetIndex ChargerInd = TargetIndex.B;

		private const TargetIndex ChargerCellInd = TargetIndex.C;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C, PathEndMode.OnCell);
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, storageMode: false);
			yield return Toils_General.Do(delegate
			{
				pawn.Map.reservationManager.Release(job.targetB, pawn, job);
				Pawn obj = (Pawn)job.targetA.Thing;
				Job newJob = JobMaker.MakeJob(targetA: (Building_MechCharger)job.targetB.Thing, def: JobDefOf.MechCharge);
				obj.jobs.StartJob(newJob, JobCondition.InterruptForced);
			});
		}
	}
}
