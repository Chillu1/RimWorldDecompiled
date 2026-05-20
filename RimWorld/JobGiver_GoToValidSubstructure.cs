using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_GoToValidSubstructure : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.mindState.duty == null)
		{
			return null;
		}
		if (pawn.GetLord() == null || !(pawn.GetLord().LordJob is LordJob_Ritual lordJob_Ritual))
		{
			return null;
		}
		Building_GravEngine building_GravEngine = lordJob_Ritual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>()?.engine;
		if (building_GravEngine == null)
		{
			return null;
		}
		IntVec3 spot;
		if (GravshipUtility.IsOnboardGravship(pawn.Position, building_GravEngine, pawn))
		{
			spot = pawn.Position;
		}
		else if (!GravshipUtility.TryFindSpotOnGravship(pawn, building_GravEngine, out spot))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.GotoShip, spot);
		job.locomotionUrgency = LocomotionUrgency.Jog;
		return job;
	}
}
