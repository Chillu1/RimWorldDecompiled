using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobDriver_RemoveBuilding : JobDriver
{
	private float workLeft;

	private float totalNeededWork;

	protected Thing Target => job.targetA.Thing;

	protected Building Building => (Building)Target.GetInnerIfMinified();

	protected abstract DesignationDef Designation { get; }

	protected abstract float TotalNeededWork { get; }

	protected abstract EffecterDef WorkEffecter { get; }

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref workLeft, "workLeft", 0f);
		Scribe_Values.Look(ref totalNeededWork, "totalNeededWork", 0f);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (Designation != null)
		{
			this.FailOnThingMissingDesignation(TargetIndex.A, Designation);
		}
		this.FailOnForbidden(TargetIndex.A);
		this.FailOn(() => Building.TryGetComp<CompExplosive>(out var comp) && comp.wickStarted);
		yield return Toils_Goto.GotoThing(TargetIndex.A, (Target is Building_Trap) ? PathEndMode.OnCell : PathEndMode.Touch);
		Toil doWork = ToilMaker.MakeToil("MakeNewToils").FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		doWork.initAction = delegate
		{
			totalNeededWork = TotalNeededWork;
			workLeft = totalNeededWork;
		};
		doWork.tickIntervalAction = delegate(int delta)
		{
			workLeft -= pawn.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f * (float)delta;
			TickActionInterval(delta);
			if (workLeft <= 0f)
			{
				doWork.actor.jobs.curDriver.ReadyForNextToil();
			}
		};
		doWork.defaultCompleteMode = ToilCompleteMode.Never;
		if (WorkEffecter != null)
		{
			doWork.WithEffect(WorkEffecter, TargetIndex.A);
		}
		doWork.WithProgressBar(TargetIndex.A, () => 1f - workLeft / totalNeededWork);
		doWork.activeSkill = () => SkillDefOf.Construction;
		yield return doWork;
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (Target.Faction != null)
			{
				Target.Faction.Notify_BuildingRemoved(Building, pawn);
			}
			FinishedRemoving();
			base.Map.designationManager.RemoveAllDesignationsOn(Target);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
	}

	protected virtual void FinishedRemoving()
	{
	}

	protected virtual void TickActionInterval(int delta)
	{
	}
}
