using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_ExtractTree : JobDriver
{
	private float workLeft;

	private float totalNeededWork;

	public const TargetIndex TreeInd = TargetIndex.A;

	protected Thing Target => job.GetTarget(TargetIndex.A).Thing;

	protected Plant Tree => (Plant)Target.GetInnerIfMinified();

	protected DesignationDef Designation => DesignationDefOf.ExtractTree;

	protected float TotalNeededWork => Tree.def.plant.harvestWork;

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
		this.FailOnThingMissingDesignation(TargetIndex.A, Designation);
		this.FailOnForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		Toil doWork = ToilMaker.MakeToil("MakeNewToils").FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		doWork.initAction = delegate
		{
			totalNeededWork = TotalNeededWork;
			workLeft = totalNeededWork;
		};
		doWork.tickIntervalAction = delegate(int delta)
		{
			workLeft -= JobDriver_PlantWork.WorkDonePerTick(pawn, Tree) * (float)delta;
			if (pawn.skills != null)
			{
				pawn.skills.Learn(SkillDefOf.Plants, 0.085f * (float)delta);
			}
			if (workLeft <= 0f)
			{
				SoundDefOf.Finish_Wood.PlayOneShot(SoundInfo.InMap(Tree));
				doWork.actor.jobs.curDriver.ReadyForNextToil();
			}
		};
		doWork.defaultCompleteMode = ToilCompleteMode.Never;
		doWork.WithProgressBar(TargetIndex.A, () => 1f - workLeft / totalNeededWork);
		doWork.WithEffect(EffecterDefOf.Harvest_Plant, TargetIndex.A);
		doWork.PlaySustainerOrSound(() => SoundDefOf.Interact_ConstructDirt);
		doWork.activeSkill = () => SkillDefOf.Plants;
		yield return doWork;
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			IntVec3 position = Tree.Position;
			bool num = Find.Selector.IsSelected(Tree);
			Thing thing = GenSpawn.Spawn(Tree.MakeMinified(), position, pawn.Map);
			if (num && thing != null)
			{
				Find.Selector.Select(thing, playSound: false, forceDesignatorDeselect: false);
			}
			base.Map.designationManager.RemoveAllDesignationsOn(Target);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
	}
}
