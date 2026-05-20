using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RemovePaintBuilding : JobDriver
{
	private float removalTime;

	private const TargetIndex PaintTargetIndex = TargetIndex.A;

	private const float PaintTimeSecondsBase = 1f;

	private Thing PaintTarget => job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil extractFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
		Toil goToBuilding = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, extractFromQueue);
		yield return extractFromQueue;
		yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
		yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
		yield return goToBuilding;
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
				if (PaintTarget is Building building)
				{
					building.ChangePaint(null);
				}
				PaintTarget.Map.designationManager.TryRemoveDesignationOn(PaintTarget, DesignationDefOf.RemovePaintBuilding);
				ReadyForNextToil();
				GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Dye), pawn.Position, pawn.Map, ThingPlaceMode.Near);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.PlaySustainerOrSound(SoundDefOf.Interact_RemovePaint);
		toil.WithProgressBar(TargetIndex.A, () => removalTime / 1f, interpolateBetweenActorAndTarget: true);
		toil.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, extractFromQueue);
		toil.handlingFacing = true;
		yield return toil;
		yield return Toils_Jump.Jump(extractFromQueue);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref removalTime, "removalTime", 0f);
	}
}
