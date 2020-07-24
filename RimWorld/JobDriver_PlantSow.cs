using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PlantSow : JobDriver
	{
		private float sowWorkDone;

		private Plant Plant => (Plant)job.GetTarget(TargetIndex.A).Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref sowWorkDone, "sowWorkDone", 0f);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_PlantSow jobDriver_PlantSow = this;
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(() => PlantUtility.AdjacentSowBlocker(jobDriver_PlantSow.job.plantDefToSow, jobDriver_PlantSow.TargetA.Cell, jobDriver_PlantSow.Map) != null).FailOn(() => !jobDriver_PlantSow.job.plantDefToSow.CanEverPlantAt_NewTemp(jobDriver_PlantSow.TargetLocA, jobDriver_PlantSow.Map));
			Toil sowToil = new Toil();
			sowToil.initAction = delegate
			{
				jobDriver_PlantSow.TargetThingA = GenSpawn.Spawn(jobDriver_PlantSow.job.plantDefToSow, jobDriver_PlantSow.TargetLocA, jobDriver_PlantSow.Map);
				jobDriver_PlantSow.pawn.Reserve(jobDriver_PlantSow.TargetThingA, sowToil.actor.CurJob);
				Plant obj = (Plant)jobDriver_PlantSow.TargetThingA;
				obj.Growth = 0f;
				obj.sown = true;
			};
			sowToil.tickAction = delegate
			{
				Pawn actor = sowToil.actor;
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Plants, 0.085f);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed);
				Plant plant2 = jobDriver_PlantSow.Plant;
				if (plant2.LifeStage != 0)
				{
					Log.Error(string.Concat(jobDriver_PlantSow, " getting sowing work while not in Sowing life stage."));
				}
				jobDriver_PlantSow.sowWorkDone += statValue;
				if (jobDriver_PlantSow.sowWorkDone >= plant2.def.plant.sowWork)
				{
					plant2.Growth = 0.05f;
					jobDriver_PlantSow.Map.mapDrawer.MapMeshDirty(plant2.Position, MapMeshFlag.Things);
					actor.records.Increment(RecordDefOf.PlantsSown);
					jobDriver_PlantSow.ReadyForNextToil();
				}
			};
			sowToil.defaultCompleteMode = ToilCompleteMode.Never;
			sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			sowToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			sowToil.WithEffect(EffecterDefOf.Sow, TargetIndex.A);
			sowToil.WithProgressBar(TargetIndex.A, () => jobDriver_PlantSow.sowWorkDone / jobDriver_PlantSow.Plant.def.plant.sowWork, interpolateBetweenActorAndTarget: true);
			sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
			sowToil.AddFinishAction(delegate
			{
				if (jobDriver_PlantSow.TargetThingA != null)
				{
					Plant plant = (Plant)sowToil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
					if (jobDriver_PlantSow.sowWorkDone < plant.def.plant.sowWork && !jobDriver_PlantSow.TargetThingA.Destroyed)
					{
						jobDriver_PlantSow.TargetThingA.Destroy();
					}
				}
			});
			sowToil.activeSkill = (() => SkillDefOf.Plants);
			yield return sowToil;
		}
	}
}
