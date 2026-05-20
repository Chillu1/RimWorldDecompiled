using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobDriver_AffectRoof : JobDriver
{
	private float workLeft;

	private const TargetIndex CellInd = TargetIndex.A;

	private const TargetIndex GotoTargetInd = TargetIndex.B;

	private const float BaseWorkAmount = 65f;

	protected IntVec3 Cell => job.GetTarget(TargetIndex.A).Cell;

	protected abstract PathEndMode PathEndMode { get; }

	protected abstract void DoEffect();

	protected abstract bool DoWorkFailOn();

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref workLeft, "workLeft", 0f);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Cell, job, 1, -1, ReservationLayerDefOf.Ceiling, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.B);
		yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode);
		Toil doWork = ToilMaker.MakeToil("MakeNewToils");
		doWork.initAction = delegate
		{
			workLeft = 65f;
		};
		doWork.tickIntervalAction = delegate(int delta)
		{
			float num = doWork.actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f * (float)delta;
			workLeft -= num;
			if (workLeft <= 0f)
			{
				DoEffect();
				ReadyForNextToil();
			}
		};
		doWork.FailOnCannotTouch(TargetIndex.B, PathEndMode);
		doWork.PlaySoundAtStart(SoundDefOf.Roof_Start);
		doWork.PlaySoundAtEnd(SoundDefOf.Roof_Finish);
		doWork.WithEffect(EffecterDefOf.RoofWork, TargetIndex.A);
		doWork.FailOn(DoWorkFailOn);
		doWork.WithProgressBar(TargetIndex.A, () => 1f - workLeft / 65f);
		doWork.defaultCompleteMode = ToilCompleteMode.Never;
		doWork.activeSkill = () => SkillDefOf.Construction;
		yield return doWork;
	}
}
