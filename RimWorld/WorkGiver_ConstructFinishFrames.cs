using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ConstructFinishFrames : WorkGiver_Scanner
{
	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame);

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

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
		if (!frame.IsCompleted())
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
		if (!GenConstruct.CanConstruct(frame, pawn, checkSkills: true, forced))
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.FinishFrame, frame);
	}
}
