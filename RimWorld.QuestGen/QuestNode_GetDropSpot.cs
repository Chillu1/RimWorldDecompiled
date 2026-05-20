using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetDropSpot : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs = "dropSpot";

		public SlateRef<float> minDistanceFromEdge;

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
			if (TryFindDropSpot(map, minDistanceFromEdge.GetValue(slate), out var spawnSpot))
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
				if (map != null && TryFindDropSpot(map, minDistanceFromEdge.GetValue(slate), out var spawnSpot))
				{
					QuestGen.slate.Set(storeAs.GetValue(slate), spawnSpot);
				}
			}
		}

		private bool TryFindDropSpot(Map map, float minDistFromEdge, out IntVec3 spawnSpot)
		{
			if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => x.Standable(map) && !x.Roofed(map) && !x.Fogged(map) && (float)x.DistanceToEdge(map) >= minDistFromEdge && map.reachability.CanReachColony(x), map, 1000, out spawnSpot))
			{
				return true;
			}
			return false;
		}
	}
}
