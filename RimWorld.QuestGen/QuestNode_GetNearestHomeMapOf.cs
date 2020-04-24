using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetNearestHomeMapOf : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs = "map";

		public SlateRef<Thing> mapOf;

		protected override bool TestRunInt(Slate slate)
		{
			DoWork(slate);
			return true;
		}

		protected override void RunInt()
		{
			DoWork(QuestGen.slate);
		}

		private void DoWork(Slate slate)
		{
			if (mapOf.GetValue(slate) == null)
			{
				return;
			}
			Map mapHeld = mapOf.GetValue(slate).MapHeld;
			if (mapHeld != null && mapHeld.IsPlayerHome)
			{
				slate.Set(storeAs.GetValue(slate), mapHeld);
				return;
			}
			int tile = mapOf.GetValue(slate).Tile;
			if (tile == -1)
			{
				return;
			}
			Map map = null;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome && (map == null || Find.WorldGrid.ApproxDistanceInTiles(tile, maps[i].Tile) < Find.WorldGrid.ApproxDistanceInTiles(tile, map.Tile)))
				{
					map = maps[i];
				}
			}
			if (map != null)
			{
				slate.Set(storeAs.GetValue(slate), map);
			}
		}
	}
}
