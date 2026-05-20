using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_BoardOrLeaveGravship : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return null;
		}
		if (pawn.Downed)
		{
			return null;
		}
		if (!(pawn.Map.listerThings.ThingsOfDef(ThingDefOf.GravEngine).FirstOrDefault() is Building_GravEngine building_GravEngine))
		{
			return null;
		}
		if (building_GravEngine.pawnsToBoard != null && building_GravEngine.pawnsToBoard.Contains(pawn))
		{
			IntVec3 spot;
			if (GravshipUtility.IsOnboardGravship_NewTemp(pawn.Position, building_GravEngine, pawn, desperate: false, respectAllowedAreas: false))
			{
				spot = pawn.Position;
			}
			else if (!GravshipUtility.TryFindSpotOnGravship(pawn, building_GravEngine, out spot))
			{
				Messages.Message("FailedToBoardGravship".Translate(pawn.Named("PAWN"), building_GravEngine.RenamableLabel), pawn, MessageTypeDefOf.NegativeEvent, historical: false);
				building_GravEngine.pawnsToBoard.Remove(pawn);
				return null;
			}
			if (pawn.lord?.LordJob is LordJob_Ritual lordJob_Ritual)
			{
				lordJob_Ritual.Cancel();
			}
			Job job = JobMaker.MakeJob(JobDefOf.GotoShip, spot);
			job.locomotionUrgency = LocomotionUrgency.Jog;
			return job;
		}
		if (building_GravEngine.pawnsToLeave != null && building_GravEngine.pawnsToLeave.Contains(pawn))
		{
			IntVec3 spot2;
			if (!GravshipUtility.IsOnboardGravship_NewTemp(pawn.Position, building_GravEngine, pawn, desperate: true, respectAllowedAreas: false))
			{
				spot2 = pawn.Position;
			}
			else if (!GravshipUtility.TryFindSpotOffGravship(pawn, building_GravEngine, out spot2))
			{
				Messages.Message("FailedToLeaveGravship".Translate(pawn.Named("PAWN"), building_GravEngine.RenamableLabel), pawn, MessageTypeDefOf.NegativeEvent, historical: false);
				building_GravEngine.pawnsToLeave.Remove(pawn);
				return null;
			}
			pawn.lord?.LordJob?.Notify_PawnLost(pawn, PawnLostCondition.ForcedByPlayerAction);
			Job job2 = JobMaker.MakeJob(JobDefOf.LeaveShip, spot2);
			job2.locomotionUrgency = LocomotionUrgency.Jog;
			return job2;
		}
		return null;
	}
}
