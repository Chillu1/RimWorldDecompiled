using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructDeliverResourcesToFrames : WorkGiver_ConstructDeliverResources
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame);

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Faction != pawn.Faction)
			{
				return null;
			}
			Frame frame = t as Frame;
			if (frame == null)
			{
				return null;
			}
			if (GenConstruct.FirstBlockingThing(frame, pawn) != null)
			{
				return GenConstruct.HandleBlockingThingJob(frame, pawn, forced);
			}
			bool checkSkills = def.workType == WorkTypeDefOf.Construction;
			if (!GenConstruct.CanConstruct(frame, pawn, checkSkills, forced))
			{
				return null;
			}
			return ResourceDeliverJobFor(pawn, frame);
		}
	}
}
