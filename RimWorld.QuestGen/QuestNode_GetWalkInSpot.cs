using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetWalkInSpot : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs = "walkInSpot";

	protected override bool TestRunInt(Slate slate)
	{
		if (slate.Exists(storeAs.GetValue(slate)))
		{
			return true;
		}
		if (!slate.Exists("map"))
		{
			return false;
		}
		Map map = slate.Get<Map>("map");
		if (TryFindWalkInSpot(map, out var spawnSpot))
		{
			slate.Set(storeAs.GetValue(slate), spawnSpot);
			return true;
		}
		return false;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (!QuestGen.slate.Exists(storeAs.GetValue(slate)))
		{
			Map map = QuestGen.slate.Get<Map>("map");
			if (map != null && TryFindWalkInSpot(map, out var spawnSpot))
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), spawnSpot);
			}
		}
	}

	private bool TryFindWalkInSpot(Map map, out IntVec3 spawnSpot)
	{
		if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !c.Fogged(map) && map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
		{
			return true;
		}
		if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
		{
			return true;
		}
		if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => true, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot))
		{
			return true;
		}
		return false;
	}
}
