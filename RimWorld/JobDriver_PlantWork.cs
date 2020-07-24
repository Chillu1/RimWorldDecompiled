using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public abstract class JobDriver_PlantWork : JobDriver
	{
		private float workDone;

		protected float xpPerTick;

		protected const TargetIndex PlantInd = TargetIndex.A;

		protected Plant Plant => (Plant)job.targetA.Thing;

		protected virtual DesignationDef RequiredDesignation => null;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			LocalTargetInfo target = job.GetTarget(TargetIndex.A);
			if (target.IsValid && !pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_PlantWork jobDriver_PlantWork = this;
			Init();
			yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(TargetIndex.A);
			Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A, (RequiredDesignation != null) ? ((Func<Thing, bool>)((Thing t) => jobDriver_PlantWork.Map.designationManager.DesignationOn(t, jobDriver_PlantWork.RequiredDesignation) != null)) : null);
			yield return initExtractTargetFromQueue;
			yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
			Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue);
			if (RequiredDesignation != null)
			{
				toil.FailOnThingMissingDesignation(TargetIndex.A, RequiredDesignation);
			}
			yield return toil;
			Toil cut = new Toil();
			cut.tickAction = delegate
			{
				Pawn actor = cut.actor;
				if (actor.skills != null)
				{
					actor.skills.Learn(SkillDefOf.Plants, jobDriver_PlantWork.xpPerTick);
				}
				float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed);
				Plant plant = jobDriver_PlantWork.Plant;
				statValue *= Mathf.Lerp(3.3f, 1f, plant.Growth);
				jobDriver_PlantWork.workDone += statValue;
				if (jobDriver_PlantWork.workDone >= plant.def.plant.harvestWork)
				{
					if (plant.def.plant.harvestedThingDef != null)
					{
						if (actor.RaceProps.Humanlike && plant.def.plant.harvestFailable && !plant.Blighted && Rand.Value > actor.GetStatValue(StatDefOf.PlantHarvestYield))
						{
							MoteMaker.ThrowText((jobDriver_PlantWork.pawn.DrawPos + plant.DrawPos) / 2f, jobDriver_PlantWork.Map, "TextMote_HarvestFailed".Translate(), 3.65f);
						}
						else
						{
							int num = plant.YieldNow();
							if (num > 0)
							{
								Thing thing = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef);
								thing.stackCount = num;
								if (actor.Faction != Faction.OfPlayer)
								{
									thing.SetForbidden(value: true);
								}
								Find.QuestManager.Notify_PlantHarvested(actor, thing);
								GenPlace.TryPlaceThing(thing, actor.Position, jobDriver_PlantWork.Map, ThingPlaceMode.Near);
								actor.records.Increment(RecordDefOf.PlantsHarvested);
							}
						}
					}
					plant.def.plant.soundHarvestFinish.PlayOneShot(actor);
					plant.PlantCollected();
					jobDriver_PlantWork.workDone = 0f;
					jobDriver_PlantWork.ReadyForNextToil();
				}
			};
			cut.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			if (RequiredDesignation != null)
			{
				cut.FailOnThingMissingDesignation(TargetIndex.A, RequiredDesignation);
			}
			cut.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			cut.defaultCompleteMode = ToilCompleteMode.Never;
			cut.WithEffect(EffecterDefOf.Harvest, TargetIndex.A);
			cut.WithProgressBar(TargetIndex.A, () => jobDriver_PlantWork.workDone / jobDriver_PlantWork.Plant.def.plant.harvestWork, interpolateBetweenActorAndTarget: true);
			cut.PlaySustainerOrSound(() => jobDriver_PlantWork.Plant.def.plant.soundHarvesting);
			cut.activeSkill = (() => SkillDefOf.Plants);
			yield return cut;
			Toil toil2 = PlantWorkDoneToil();
			if (toil2 != null)
			{
				yield return toil2;
			}
			yield return Toils_Jump.Jump(initExtractTargetFromQueue);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref workDone, "workDone", 0f);
		}

		protected virtual void Init()
		{
		}

		protected virtual Toil PlantWorkDoneToil()
		{
			return null;
		}
	}
}
