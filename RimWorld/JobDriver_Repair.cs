using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Repair : JobDriver
{
	protected float ticksToNextRepair;

	private const float WarmupTicks = 80f;

	private const float TicksBetweenRepairs = 20f;

	private Building Building => (Building)job.GetTarget(TargetIndex.A).Thing;

	private bool IsBuildingAttachment
	{
		get
		{
			ThingDef thingDef = GenConstruct.BuiltDefOf(Building.def) as ThingDef;
			if (thingDef?.building != null)
			{
				return thingDef.building.isAttachment;
			}
			return false;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		PathEndMode pathEndMode = (IsBuildingAttachment ? PathEndMode.OnCell : PathEndMode.Touch);
		yield return Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);
		Toil repair = ToilMaker.MakeToil("MakeNewToils");
		repair.initAction = delegate
		{
			ticksToNextRepair = 80f;
		};
		repair.tickAction = delegate
		{
			Pawn actor = repair.actor;
			actor.skills?.Learn(SkillDefOf.Construction, 0.05f);
			if (IsBuildingAttachment)
			{
				actor.rotationTracker.FaceTarget(GenConstruct.GetWallAttachedTo(Building));
			}
			else
			{
				actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));
			}
			float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f;
			ticksToNextRepair -= num;
			if (ticksToNextRepair <= 0f)
			{
				ticksToNextRepair += 20f;
				base.TargetThingA.HitPoints++;
				base.TargetThingA.HitPoints = Mathf.Min(base.TargetThingA.HitPoints, base.TargetThingA.MaxHitPoints);
				base.Map.listerBuildingsRepairable.Notify_BuildingRepaired((Building)base.TargetThingA);
				if (base.TargetThingA.HitPoints == base.TargetThingA.MaxHitPoints)
				{
					actor.records.Increment(RecordDefOf.ThingsRepaired);
					actor.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
			}
		};
		repair.FailOnCannotTouch(TargetIndex.A, pathEndMode);
		repair.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
		repair.defaultCompleteMode = ToilCompleteMode.Never;
		repair.activeSkill = () => SkillDefOf.Construction;
		repair.handlingFacing = true;
		yield return repair;
	}
}
