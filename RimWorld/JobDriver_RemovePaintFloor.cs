using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RemovePaintFloor : JobDriver
{
	private float removalTime;

	private const TargetIndex PaintTargetIndex = TargetIndex.A;

	private const float RemoveTimeSecondsBase = 1f;

	private IntVec3 PaintTarget => job.GetTarget(TargetIndex.A).Cell;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil checkFinished = Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
		yield return checkFinished;
		yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).JumpIf(() => base.Map.designationManager.DesignationAt(base.TargetLocA, DesignationDefOf.RemovePaintFloor) == null, checkFinished);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			removalTime = 0f;
		};
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceTarget(PaintTarget);
			removalTime += pawn.GetStatValue(StatDefOf.WorkSpeedGlobal) / 60f * (float)delta;
			if (removalTime >= 1f)
			{
				base.Map.terrainGrid.SetTerrainColor(PaintTarget, null);
				base.Map.designationManager.TryRemoveDesignation(PaintTarget, DesignationDefOf.RemovePaintFloor);
				ReadyForNextToil();
				GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Dye), pawn.Position, pawn.Map, ThingPlaceMode.Near);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.PlaySustainerOrSound(SoundDefOf.Interact_RemovePaint);
		toil.WithProgressBar(TargetIndex.A, () => removalTime / 1f, interpolateBetweenActorAndTarget: true);
		toil.JumpIf(() => base.Map.designationManager.DesignationAt(base.TargetLocA, DesignationDefOf.RemovePaintFloor) == null, checkFinished);
		toil.handlingFacing = true;
		yield return toil;
		yield return Toils_Jump.Jump(checkFinished);
	}
}
