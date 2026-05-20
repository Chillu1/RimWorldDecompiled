using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobDriver_AffectFloor : JobDriver
{
	private float workLeft = -1000f;

	protected bool clearSnow;

	protected abstract int BaseWorkAmount { get; }

	protected abstract DesignationDef DesDef { get; }

	protected virtual StatDef SpeedStat => null;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, ReservationLayerDefOf.Floor, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => (!job.ignoreDesignations && base.Map.designationManager.DesignationAt(base.TargetLocA, DesDef) == null) ? true : false);
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
		Toil doWork = ToilMaker.MakeToil("MakeNewToils");
		doWork.initAction = delegate
		{
			workLeft = BaseWorkAmount;
		};
		doWork.tickIntervalAction = delegate(int delta)
		{
			float num = ((SpeedStat != null && !SpeedStat.Worker.IsDisabledFor(doWork.actor)) ? doWork.actor.GetStatValue(SpeedStat) : 1f);
			num *= 1.7f * (float)delta;
			workLeft -= num;
			if (doWork.actor.skills != null)
			{
				doWork.actor.skills.Learn(SkillDefOf.Construction, 0.1f * (float)delta);
			}
			if (clearSnow)
			{
				base.Map.snowGrid.SetDepth(base.TargetLocA, 0f);
				base.Map.sandGrid?.SetDepth(base.TargetLocA, 0f);
			}
			if (workLeft <= 0f)
			{
				DoEffect(base.TargetLocA);
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

	protected abstract void DoEffect(IntVec3 c);

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref workLeft, "workLeft", 0f);
	}
}
