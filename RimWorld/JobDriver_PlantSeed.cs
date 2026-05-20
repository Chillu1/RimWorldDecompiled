using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PlantSeed : JobDriver
{
	private float sowWorkDone;

	private const TargetIndex SeedIndex = TargetIndex.A;

	private const TargetIndex PlantCellIndex = TargetIndex.B;

	private const TargetIndex OldSeedStackIndex = TargetIndex.C;

	private IntVec3 PlantCell => job.GetTarget(TargetIndex.B).Cell;

	private CompPlantable PlantableComp => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompPlantable>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, 1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(() => PlantUtility.AdjacentSowBlocker(job.plantDefToSow, PlantCell, base.Map) != null || !job.plantDefToSow.CanEverPlantAt(PlantCell, base.Map, canWipePlantsExceptTree: false, checkMapTemperature: false) || !PlantableComp.PlantCells.Contains(PlantCell));
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.Do(delegate
		{
			LocalTargetInfo target = job.GetTarget(TargetIndex.A);
			if (target.HasThing && target.Thing.stackCount > job.count)
			{
				job.SetTarget(TargetIndex.C, target.Thing);
			}
		});
		yield return Toils_Haul.StartCarryThing(TargetIndex.A);
		yield return Toils_General.Do(delegate
		{
			LocalTargetInfo target = job.GetTarget(TargetIndex.C);
			if (target.IsValid && target.HasThing)
			{
				target.Thing.TryGetComp<CompPlantable>()?.Notify_SeedRemovedFromStackForPlantingAt(PlantCell);
			}
			PlantableComp?.Notify_RemovedFromStackForPlantingAt(PlantCell);
		});
		yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.B, PathEndMode.Touch);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			sowWorkDone += pawn.GetStatValue(StatDefOf.PlantWorkSpeed) * (float)delta;
			if (pawn.skills != null)
			{
				pawn.skills.Learn(SkillDefOf.Plants, 0.085f * (float)delta);
			}
			if (sowWorkDone >= job.plantDefToSow.plant.sowWork)
			{
				ReadyForNextToil();
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.WithEffect(EffecterDefOf.Sow, TargetIndex.B);
		toil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
		toil.WithProgressBar(TargetIndex.B, () => sowWorkDone / job.plantDefToSow.plant.sowWork, interpolateBetweenActorAndTarget: true);
		toil.activeSkill = () => SkillDefOf.Plants;
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			PlantableComp.DoPlant(pawn, PlantCell, pawn.Map);
		});
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref sowWorkDone, "sowWorkDone", 0f);
	}
}
