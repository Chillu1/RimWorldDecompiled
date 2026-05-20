using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ConstructDeliverResourcesToBlueprints : WorkGiver_ConstructDeliverResources
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Blueprint);

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.Faction != pawn.Faction)
		{
			return null;
		}
		if (!(t is Blueprint blueprint) || blueprint.def.entityDefToBuild is ThingDef { plant: not null })
		{
			return null;
		}
		if (!GenConstruct.CanTouchTargetFromValidCell(blueprint, pawn))
		{
			return null;
		}
		if (GenConstruct.FirstBlockingThing(blueprint, pawn) != null)
		{
			return GenConstruct.HandleBlockingThingJob(blueprint, pawn, forced);
		}
		if (!GenConstruct.CanConstruct(blueprint, pawn, def.workType, forced, JobDefOf.HaulToContainer))
		{
			return null;
		}
		if (def.workType != WorkTypeDefOf.Construction && WorkGiver_ConstructDeliverResources.ShouldRemoveExistingFloorFirst(pawn, blueprint))
		{
			return null;
		}
		Job job = RemoveExistingFloorJob(pawn, blueprint);
		if (job != null)
		{
			return job;
		}
		Job job2 = ResourceDeliverJobFor(pawn, blueprint, canRemoveExistingFloorUnderNearbyNeeders: true, forced);
		if (job2 != null)
		{
			return job2;
		}
		if (def.workType != WorkTypeDefOf.Hauling)
		{
			Job job3 = NoCostFrameMakeJobFor(blueprint);
			if (job3 != null)
			{
				return job3;
			}
		}
		return null;
	}

	private Job NoCostFrameMakeJobFor(IConstructible c)
	{
		if (c is Blueprint_Install)
		{
			return null;
		}
		if (c is Blueprint && c.TotalMaterialCost().Count == 0)
		{
			Job job = JobMaker.MakeJob(JobDefOf.PlaceNoCostFrame);
			job.targetA = (Thing)c;
			return job;
		}
		return null;
	}
}
