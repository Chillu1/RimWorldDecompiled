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
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOn(() => PlantUtility.AdjacentSowBlocker(job.plantDefToSow, base.TargetA.Cell, base.Map) != null).FailOn(() => !job.plantDefToSow.CanEverPlantAt_NewTemp(base.TargetLocA, base.Map));
			Toil sowToil = new Toil();
			sowToil.initAction = delegate
			{
				base.TargetThingA = GenSpawn.Spawn(job.plantDefToSow, base.TargetLocA, base.Map);
				pawn.Reserve(base.TargetThingA, sowToil.actor.CurJob);
				Plant obj = (Plant)base.TargetThingA;
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
				Plant plant2 = Plant;
				if (plant2.LifeStage != 0)
				{
					Log.Error(string.Concat(this, " getting sowing work while not in Sowing life stage."));
				}
				sowWorkDone += statValue;
				if (sowWorkDone >= plant2.def.plant.sowWork)
				{
					plant2.Growth = 0.05f;
					base.Map.mapDrawer.MapMeshDirty(plant2.Position, MapMeshFlag.Things);
					actor.records.Increment(RecordDefOf.PlantsSown);
					ReadyForNextToil();
				}
			};
			sowToil.defaultCompleteMode = ToilCompleteMode.Never;
			sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			sowToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			sowToil.WithEffect(EffecterDefOf.Sow, TargetIndex.A);
			sowToil.WithProgressBar(TargetIndex.A, () => sowWorkDone / Plant.def.plant.sowWork, interpolateBetweenActorAndTarget: true);
			sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
			sowToil.AddFinishAction(delegate
			{
				if (base.TargetThingA != null)
				{
					Plant plant = (Plant)sowToil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
					if (sowWorkDone < plant.def.plant.sowWork && !base.TargetThingA.Destroyed)
					{
						base.TargetThingA.Destroy();
					}
				}
			});
			sowToil.activeSkill = () => SkillDefOf.Plants;
			yield return sowToil;
		}
	}
}
