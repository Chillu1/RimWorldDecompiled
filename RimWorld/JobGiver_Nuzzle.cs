using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Nuzzle : ThinkNode_JobGiver
{
	private const float MaxNuzzleDistance = 40f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (NuzzleUtility.GetNuzzleMTBHours(pawn) <= 0f)
		{
			return null;
		}
		if (!(from p in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
			where !p.NonHumanlikeOrWildMan() && !p.IsSubhuman && p != pawn && p.Position.InHorDistOf(pawn.Position, 40f) && pawn.GetRoom() == p.GetRoom() && !p.Position.IsForbidden(pawn) && p.CanCasuallyInteractNow()
			select p).TryRandomElement(out var result))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Nuzzle, result);
		job.locomotionUrgency = LocomotionUrgency.Walk;
		job.expiryInterval = 3000;
		return job;
	}
}
