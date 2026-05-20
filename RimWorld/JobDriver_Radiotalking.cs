using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Radiotalking : JobDriver
{
	public Building_CommsConsole CommsConsole => (Building_CommsConsole)base.TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(base.TargetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		this.FailOnChildLearningConditions();
		this.FailOn(() => !CommsConsole.CanUseCommsNow);
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(() => !CommsConsole.CanUseCommsNow);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceTarget(base.TargetA);
			LearningUtility.LearningTickCheckEnd(pawn, delta);
		};
		toil.WithEffect(EffecterDefOf.Radiotalking, TargetIndex.A);
		toil.handlingFacing = true;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		yield return toil;
	}
}
