using Verse;

namespace RimWorld;

public class JobGiver_UnnaturalCorpseSkipBackup : JobGiver_UnnaturalCorpseSkip
{
	protected override bool TryGetSkipCell(Pawn pawn, Pawn victim, out IntVec3 cell)
	{
		if (TryGetCellAlongPath(pawn, victim, 64, out cell))
		{
			return true;
		}
		if (TryGetNearbySkipCell(pawn, victim.PositionHeld, victim, 48, out cell))
		{
			return true;
		}
		cell = victim.Position;
		return true;
	}
}
