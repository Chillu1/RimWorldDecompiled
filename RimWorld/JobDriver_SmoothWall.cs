using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
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
			JobDriver_SmoothWall jobDriver_SmoothWall = this;
			this.FailOn(() => (!jobDriver_SmoothWall.job.ignoreDesignations && jobDriver_SmoothWall.Map.designationManager.DesignationAt(jobDriver_SmoothWall.TargetLocA, jobDriver_SmoothWall.DesDef) == null) ? true : false);
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				jobDriver_SmoothWall.workLeft = jobDriver_SmoothWall.BaseWorkAmount;
			};
			doWork.tickAction = delegate
			{
				float num = doWork.actor.GetStatValue(StatDefOf.SmoothingSpeed) * 1.7f;
				jobDriver_SmoothWall.workLeft -= num;
				if (doWork.actor.skills != null)
				{
					doWork.actor.skills.Learn(SkillDefOf.Construction, 0.1f);
				}
				if (jobDriver_SmoothWall.workLeft <= 0f)
				{
					jobDriver_SmoothWall.DoEffect();
					jobDriver_SmoothWall.Map.designationManager.DesignationAt(jobDriver_SmoothWall.TargetLocA, jobDriver_SmoothWall.DesDef)?.Delete();
					jobDriver_SmoothWall.ReadyForNextToil();
				}
			};
			doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			doWork.WithProgressBar(TargetIndex.A, () => 1f - jobDriver_SmoothWall.workLeft / (float)jobDriver_SmoothWall.BaseWorkAmount);
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.activeSkill = (() => SkillDefOf.Construction);
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
}
