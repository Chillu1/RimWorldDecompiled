using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_UnnaturalCorpseUnstuck : JobGiver_UnnaturalCorpseSkip
{
	private const int StuckTicks = 30;

	private const int StuckTeleportToTicks = 300;

	private const int MaxDist = 10;

	private const int MinDist = 2;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.Downed)
		{
			return null;
		}
		if (GenTicks.TicksGame - pawn.TickSpawned <= 30)
		{
			return null;
		}
		if (pawn.pather.MovedRecently(30))
		{
			return null;
		}
		return base.TryGiveJob(pawn);
	}

	protected override bool TryGetSkipCell(Pawn pawn, Pawn victim, out IntVec3 cell)
	{
		if (TryGetCellAlongPath(pawn, victim, 10, out cell))
		{
			return true;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(pawn.Position, 2f, 10f))
		{
			if (ValidateCell(pawn, item, victim.PositionHeld, pawn.Map))
			{
				cell = item;
				return true;
			}
		}
		if (!pawn.pather.MovedRecently(300))
		{
			cell = victim.PositionHeld;
			return true;
		}
		cell = IntVec3.Invalid;
		return false;
	}

	private bool ValidateCell(Pawn pawn, IntVec3 start, IntVec3 goal, Map map)
	{
		if (!start.Standable(map))
		{
			return false;
		}
		if (start.GetFirstPawn(map) != null)
		{
			return false;
		}
		float num = pawn.Position.DistanceTo(goal);
		if (start.DistanceTo(goal) + 1f >= num)
		{
			return false;
		}
		return pawn.CanReach(start, goal, PathEndMode.Touch, Danger.Deadly);
	}
}
