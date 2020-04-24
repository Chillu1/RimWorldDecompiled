using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetLargestClearArea : QuestNode
	{
		public SlateRef<Map> map;

		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<int> failIfSmaller;

		public SlateRef<int> max;

		protected override bool TestRunInt(Slate slate)
		{
			int largestSize = GetLargestSize(slate);
			slate.Set(storeAs.GetValue(slate), largestSize);
			return largestSize >= failIfSmaller.GetValue(slate);
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			int largestSize = GetLargestSize(slate);
			slate.Set(storeAs.GetValue(slate), largestSize);
		}

		private int GetLargestSize(Slate slate)
		{
			Map mapResolved = map.GetValue(slate) ?? slate.Get<Map>("map");
			if (mapResolved == null)
			{
				return 0;
			}
			int value = max.GetValue(slate);
			CellRect cellRect = LargestAreaFinder.FindLargestRect(mapResolved, (IntVec3 x) => IsClear(x, mapResolved), value);
			return Mathf.Min(cellRect.Width, cellRect.Height, value);
		}

		private bool IsClear(IntVec3 c, Map map)
		{
			if (!c.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.IsBuildingArtificial && thingList[i].Faction == Faction.OfPlayer)
				{
					return false;
				}
				if (!thingList[i].def.mineable)
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < 8; j++)
				{
					IntVec3 c2 = c + GenAdj.AdjacentCells[j];
					if (c2.InBounds(map) && c2.GetFirstMineable(map) == null)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}
	}
}
