using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_SmoothWall : JobDriver
{
	private float workLeft = -1000f;

	protected int BaseWorkAmount => 6500;

	protected DesignationDef DesDef => DesignationDefOf.SmoothWall;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(job.targetA.Cell, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => (!job.ignoreDesignations && base.Map.designationManager.DesignationAt(base.TargetLocA, DesDef) == null) ? true : false);
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
		Toil doWork = ToilMaker.MakeToil("MakeNewToils");
		doWork.initAction = delegate
		{
			workLeft = BaseWorkAmount;
		};
		doWork.tickIntervalAction = delegate(int delta)
		{
			float num = doWork.actor.GetStatValue(StatDefOf.SmoothingSpeed) * 1.7f * (float)delta;
			workLeft -= num;
			if (doWork.actor.skills != null)
			{
				doWork.actor.skills.Learn(SkillDefOf.Construction, 0.1f * (float)delta);
			}
			if (workLeft <= 0f)
			{
				DoEffect();
				base.Map.designationManager.DesignationAt(base.TargetLocA, DesDef)?.Delete();
				ReadyForNextToil();
			}
		};
		doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		doWork.WithProgressBar(TargetIndex.A, () => 1f - workLeft / (float)BaseWorkAmount);
		doWork.defaultCompleteMode = ToilCompleteMode.Never;
		doWork.activeSkill = () => SkillDefOf.Construction;
		yield return doWork;
	}

	protected void DoEffect()
	{
		SmoothableWallUtility.Notify_SmoothedByPawn(SmoothableWallUtility.SmoothWall(base.TargetA.Thing, pawn), pawn);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref workLeft, "workLeft", 0f);
	}
}
