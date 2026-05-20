using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_TakeInventory : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 10, job.count, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOn(() => job.checkEncumbrance && MassUtility.CountToPickUpUntilOverEncumbered(pawn, job.targetA.Thing) == 0);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			pawn.pather.StartPath(base.TargetThingA, PathEndMode.ClosestTouch);
		};
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return toil;
		if (job.takeInventoryDelay > 0)
		{
			Toil toil2 = Toils_General.Wait(job.takeInventoryDelay);
			toil2.WithProgressBarToilDelay(TargetIndex.A);
			toil2.tickIntervalAction = delegate
			{
				pawn.rotationTracker.FaceTarget(base.TargetThingA);
			};
			toil2.handlingFacing = true;
			yield return toil2;
		}
		yield return Toils_Haul.TakeToInventory(TargetIndex.A, job.count);
	}
}
