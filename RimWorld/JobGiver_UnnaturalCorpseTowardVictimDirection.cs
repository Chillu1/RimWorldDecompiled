using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_UnnaturalCorpseTowardVictimDirection : ThinkNode_JobGiver
{
	private const float LookAheadDist = 10f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.Downed)
		{
			return null;
		}
		if (!Find.Anomaly.TryGetUnnaturalCorpseTrackerForAwoken(pawn, out var tracker))
		{
			return null;
		}
		Pawn haunted = tracker.Haunted;
		if (haunted.DestroyedOrNull())
		{
			return null;
		}
		Vector3 normalized = (haunted.PositionHeld - pawn.Position).ToVector3().normalized;
		IntVec3 ideal = pawn.Position + (normalized * 10f).ToIntVec3();
		if (TryGetNearbyCell(pawn, ideal, out var cell))
		{
			return JobMaker.MakeJob(JobDefOf.Goto, cell);
		}
		return null;
	}

	private bool TryGetNearbyCell(Pawn pawn, IntVec3 ideal, out IntVec3 cell)
	{
		return RCellFinder.TryFindRandomCellNearWith(ideal, (IntVec3 x) => x.Standable(pawn.Map) && pawn.CanReach(pawn.Position, x, PathEndMode.OnCell, Danger.Deadly), pawn.Map, out cell);
	}
}
