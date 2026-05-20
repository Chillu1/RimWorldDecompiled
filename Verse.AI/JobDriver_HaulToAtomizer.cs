using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_HaulToAtomizer : JobDriver
	{
		private const TargetIndex AtomizerInd = TargetIndex.A;

		private const TargetIndex WastepackInd = TargetIndex.B;

		private const TargetIndex ChargerCellInd = TargetIndex.C;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			AddEndCondition(() => (base.TargetThingA.TryGetComp<CompAtomizer>().SpaceLeft > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);
			Toil clearQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.B);
			yield return clearQueue;
			yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.B);
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(clearQueue, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.A, TargetIndex.B);
			yield return Toils_Jump.Jump(clearQueue);
		}
	}
}
