using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ConstructFinishFrame : JobDriver, IBuildableDriver
{
	private const int JobEndInterval = 5000;

	private const TargetIndex BuildingInd = TargetIndex.A;

	private Frame Frame => (Frame)job.GetTarget(TargetIndex.A).Thing;

	private bool IsBuildingAttachment => ((GenConstruct.BuiltDefOf(Frame.def) as ThingDef)?.building)?.isAttachment ?? false;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	public bool TryGetBuildableRect(out CellRect rect)
	{
		rect = Frame.OccupiedRect();
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (IsBuildingAttachment)
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		}
		else
		{
			yield return Toils_Goto.GotoBuild(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.A);
		}
		Toil build = ToilMaker.MakeToil("MakeNewToils");
		build.initAction = delegate
		{
			GenClamor.DoClamor(build.actor, 15f, ClamorDefOf.Construction);
		};
		build.tickIntervalAction = delegate(int delta)
		{
			Pawn actor = build.actor;
			Frame frame = Frame;
			if (frame.resourceContainer.Count > 0 && actor.skills != null)
			{
				actor.skills.Learn(SkillDefOf.Construction, 0.25f * (float)delta);
			}
			actor.rotationTracker.FaceTarget(IsBuildingAttachment ? GenConstruct.GetWallAttachedTo(frame) : frame);
			float num = actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f * (float)delta;
			if (frame.Stuff != null)
			{
				num *= frame.Stuff.GetStatValueAbstract(StatDefOf.ConstructionSpeedFactor);
			}
			float workToBuild = frame.WorkToBuild;
			if (actor.Faction == Faction.OfPlayer)
			{
				float statValue = actor.GetStatValue(StatDefOf.ConstructSuccessChance);
				if (!TutorSystem.TutorialMode && Rand.Value < 1f - Mathf.Pow(statValue, num / workToBuild))
				{
					frame.FailConstruction(actor);
					ReadyForNextToil();
					return;
				}
			}
			if (frame.def.entityDefToBuild is TerrainDef)
			{
				base.Map.snowGrid.SetDepth(frame.Position, 0f);
				base.Map.sandGrid?.SetDepth(frame.Position, 0f);
			}
			frame.workDone += num;
			if (frame.workDone >= workToBuild)
			{
				frame.CompleteConstruction(actor);
				ReadyForNextToil();
			}
		};
		build.WithEffect(() => ((Frame)build.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).ConstructionEffect, TargetIndex.A);
		build.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		build.FailOn(() => !GenConstruct.CanConstruct(Frame, pawn));
		build.defaultCompleteMode = ToilCompleteMode.Delay;
		build.defaultDuration = 5000;
		build.activeSkill = () => SkillDefOf.Construction;
		build.handlingFacing = true;
		yield return build;
	}
}
