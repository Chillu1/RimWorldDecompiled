using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ConstructDeliverResourcesToFrames : WorkGiver_ConstructDeliverResources
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame);

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.Faction != pawn.Faction)
		{
			return null;
		}
		if (!(t is Frame frame))
		{
			return null;
		}
		if (!GenConstruct.CanTouchTargetFromValidCell(frame, pawn))
		{
			return null;
		}
		if (GenConstruct.FirstBlockingThing(frame, pawn) != null)
		{
			return GenConstruct.HandleBlockingThingJob(frame, pawn, forced);
		}
		if (!GenConstruct.CanConstruct(frame, pawn, def.workType, forced, JobDefOf.HaulToContainer))
		{
			return null;
		}
		return ResourceDeliverJobFor(pawn, frame, canRemoveExistingFloorUnderNearbyNeeders: true, forced);
	}
}
