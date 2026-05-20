using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PaintFloor : JobDriver
{
	private float paintingTime;

	private const TargetIndex PaintTargetIndex = TargetIndex.A;

	private const TargetIndex DyeIndex = TargetIndex.B;

	private const float PaintTimeSecondsBase = 1f;

	private IntVec3 PaintTarget => job.GetTarget(TargetIndex.A).Cell;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job, 1, -1, ReservationLayerDefOf.Floor);
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil extractFromQueue = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
		Toil checkFinished = Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
		yield return Toils_Jump.JumpIf(extractFromQueue, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
		foreach (Toil item in CollectDyeToils())
		{
			yield return item;
		}
		yield return checkFinished;
		yield return extractFromQueue;
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).JumpIf(() => base.Map.designationManager.DesignationAt(base.TargetLocA, DesignationDefOf.PaintFloor) == null, checkFinished);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			paintingTime = 0f;
		};
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceTarget(PaintTarget);
			paintingTime += pawn.GetStatValue(StatDefOf.WorkSpeedGlobal) / 60f * (float)delta;
			pawn.skills?.Learn(SkillDefOf.Artistic, 0.1f * (float)delta);
			if (paintingTime >= 1f)
			{
				pawn.carryTracker.CarriedThing?.SplitOff(1)?.Destroy();
				Designation designation = base.Map.designationManager.DesignationAt(PaintTarget, DesignationDefOf.PaintFloor);
				if (designation != null)
				{
					base.Map.terrainGrid.SetTerrainColor(PaintTarget, designation.colorDef);
					base.Map.designationManager.RemoveDesignation(designation);
				}
				ReadyForNextToil();
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.WithEffect(EffecterDefOf.Paint, TargetIndex.A);
		toil.WithProgressBar(TargetIndex.A, () => paintingTime / 1f, interpolateBetweenActorAndTarget: true);
		toil.JumpIf(() => base.Map.designationManager.DesignationAt(base.TargetLocA, DesignationDefOf.PaintFloor) == null, checkFinished);
		toil.activeSkill = () => SkillDefOf.Artistic;
		toil.handlingFacing = true;
		yield return toil;
		yield return Toils_Jump.Jump(checkFinished);
	}

	private IEnumerable<Toil> CollectDyeToils()
	{
		Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, failIfCountFromQueueTooBig: false);
		yield return extract;
		Toil jumpIfHaveTargetInQueue = Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, extract);
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: true);
		yield return jumpIfHaveTargetInQueue;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref paintingTime, "paintingTime", 0f);
	}
}
