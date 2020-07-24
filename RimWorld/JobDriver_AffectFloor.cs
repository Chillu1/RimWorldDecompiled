using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobDriver_AffectFloor : JobDriver
	{
		private float workLeft = -1000f;

		protected bool clearSnow;

		protected abstract int BaseWorkAmount
		{
			get;
		}

		protected abstract DesignationDef DesDef
		{
			get;
		}

		protected virtual StatDef SpeedStat => null;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, ReservationLayerDefOf.Floor, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_AffectFloor jobDriver_AffectFloor = this;
			this.FailOn(() => (!jobDriver_AffectFloor.job.ignoreDesignations && jobDriver_AffectFloor.Map.designationManager.DesignationAt(jobDriver_AffectFloor.TargetLocA, jobDriver_AffectFloor.DesDef) == null) ? true : false);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				jobDriver_AffectFloor.workLeft = jobDriver_AffectFloor.BaseWorkAmount;
			};
			doWork.tickAction = delegate
			{
				float num = (jobDriver_AffectFloor.SpeedStat != null) ? doWork.actor.GetStatValue(jobDriver_AffectFloor.SpeedStat) : 1f;
				num *= 1.7f;
				jobDriver_AffectFloor.workLeft -= num;
				if (doWork.actor.skills != null)
				{
					doWork.actor.skills.Learn(SkillDefOf.Construction, 0.1f);
				}
				if (jobDriver_AffectFloor.clearSnow)
				{
					jobDriver_AffectFloor.Map.snowGrid.SetDepth(jobDriver_AffectFloor.TargetLocA, 0f);
				}
				if (jobDriver_AffectFloor.workLeft <= 0f)
				{
					jobDriver_AffectFloor.DoEffect(jobDriver_AffectFloor.TargetLocA);
					jobDriver_AffectFloor.Map.designationManager.DesignationAt(jobDriver_AffectFloor.TargetLocA, jobDriver_AffectFloor.DesDef)?.Delete();
					jobDriver_AffectFloor.ReadyForNextToil();
				}
			};
			doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			doWork.WithProgressBar(TargetIndex.A, () => 1f - jobDriver_AffectFloor.workLeft / (float)jobDriver_AffectFloor.BaseWorkAmount);
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.activeSkill = (() => SkillDefOf.Construction);
			yield return doWork;
		}

		protected abstract void DoEffect(IntVec3 c);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref workLeft, "workLeft", 0f);
		}
	}
}
