using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Repair : JobDriver
	{
		protected float ticksToNextRepair;

		private const float WarmupTicks = 80f;

		private const float TicksBetweenRepairs = 20f;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			JobDriver_Repair jobDriver_Repair = this;
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil repair = new Toil();
			repair.initAction = delegate
			{
				jobDriver_Repair.ticksToNextRepair = 80f;
			};
			repair.tickAction = delegate
			{
				Pawn actor = repair.actor;
				actor.skills.Learn(SkillDefOf.Construction, 0.05f);
				float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f;
				jobDriver_Repair.ticksToNextRepair -= num;
				if (jobDriver_Repair.ticksToNextRepair <= 0f)
				{
					jobDriver_Repair.ticksToNextRepair += 20f;
					jobDriver_Repair.TargetThingA.HitPoints++;
					jobDriver_Repair.TargetThingA.HitPoints = Mathf.Min(jobDriver_Repair.TargetThingA.HitPoints, jobDriver_Repair.TargetThingA.MaxHitPoints);
					jobDriver_Repair.Map.listerBuildingsRepairable.Notify_BuildingRepaired((Building)jobDriver_Repair.TargetThingA);
					if (jobDriver_Repair.TargetThingA.HitPoints == jobDriver_Repair.TargetThingA.MaxHitPoints)
					{
						actor.records.Increment(RecordDefOf.ThingsRepaired);
						actor.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				}
			};
			repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			repair.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
			repair.defaultCompleteMode = ToilCompleteMode.Never;
			repair.activeSkill = (() => SkillDefOf.Construction);
			yield return repair;
		}
	}
}
