using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_PlayWithGoldenCube : ThinkNode_JobGiver
{
	private const float SeverityToOverrideDrafted = 0.9f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.Downed || pawn.Drafted)
		{
			return null;
		}
		if (!pawn.health.hediffSet.TryGetHediff(HediffDefOf.CubeWithdrawal, out var hediff))
		{
			return null;
		}
		if (hediff.Severity < 0.9f && pawn.Drafted)
		{
			return null;
		}
		if (pawn.mindState.duty != null)
		{
			return null;
		}
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return null;
		}
		if (!TryGetNearestCube(pawn, out var cube))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.GoldenCubePlay, cube);
		job.count = 1;
		return job;
	}

	public static bool TryGetNearestCube(Pawn pawn, out Thing cube)
	{
		if (!pawn.Spawned)
		{
			cube = null;
			return false;
		}
		cube = GenClosest.ClosestThingReachable(pawn.PositionHeld, pawn.MapHeld, ThingRequest.ForDef(ThingDefOf.GoldenCube), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing t) => IsValid(t, pawn));
		return cube != null;
	}

	private static bool IsValid(Thing thing, Pawn pawn)
	{
		if (thing.def != ThingDefOf.GoldenCube)
		{
			return false;
		}
		if (thing.IsForbidden(pawn))
		{
			return false;
		}
		if (!pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None))
		{
			return false;
		}
		return true;
	}
}
