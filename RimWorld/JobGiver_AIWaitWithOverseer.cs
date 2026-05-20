using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIWaitWithOverseer : ThinkNode_JobGiver
{
	private const float RandomCellNearRadius = 1.9f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn overseer = pawn.GetOverseer();
		if (overseer == null || overseer.Awake())
		{
			return null;
		}
		IntVec3 intVec = (CanUseCell(pawn.Position, pawn) ? pawn.Position : GetWaitDest(overseer.Position, pawn));
		if (intVec.IsValid)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Wait_WithSleeping, intVec, overseer);
			job.expiryInterval = 120;
			job.expireRequiresEnemiesNearby = true;
			return job;
		}
		return null;
	}

	private IntVec3 GetWaitDest(IntVec3 root, Pawn mech)
	{
		Map map = mech.Map;
		if (CellFinder.TryFindRandomReachableNearbyCell(root, map, 1.9f, TraverseParms.For(mech), (IntVec3 c) => CanUseCell(c, mech), null, out var result))
		{
			return result;
		}
		return IntVec3.Invalid;
	}

	private bool CanUseCell(IntVec3 c, Pawn mech)
	{
		Map map = mech.Map;
		if (c.Standable(map) && mech.CanReach(c, PathEndMode.OnCell, Danger.Deadly) && mech.CanReserve(c))
		{
			return c.GetDoor(map) == null;
		}
		return false;
	}
}
