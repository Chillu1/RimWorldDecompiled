using Verse;

namespace RimWorld;

public class ThoughtWorker_Recluse : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!p.Spawned)
		{
			return ThoughtState.Inactive;
		}
		if (ThoughtUtility.ThoughtNullified(p, def))
		{
			return ThoughtState.Inactive;
		}
		int freeColonistsAndPrisonersSpawnedCount = p.Map.mapPawns.FreeColonistsAndPrisonersSpawnedCount;
		if (freeColonistsAndPrisonersSpawnedCount <= 1)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		if (freeColonistsAndPrisonersSpawnedCount <= 4)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		if (freeColonistsAndPrisonersSpawnedCount <= 10)
		{
			return ThoughtState.Inactive;
		}
		if (freeColonistsAndPrisonersSpawnedCount <= 15)
		{
			return ThoughtState.ActiveAtStage(2);
		}
		return ThoughtState.ActiveAtStage(3);
	}
}
