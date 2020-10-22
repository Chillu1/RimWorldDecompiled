using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetMap : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs = "map";

		public SlateRef<bool> mustBeInfestable;

		public SlateRef<int> preferMapWithMinFreeColonists;

		protected override bool TestRunInt(Slate slate)
		{
			if (slate.TryGet<Map>(storeAs.GetValue(slate), out var var) && IsAcceptableMap(var, slate))
			{
				return true;
			}
			if (TryFindMap(slate, out var))
			{
				slate.Set(storeAs.GetValue(slate), var);
				return true;
			}
			return false;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if ((!QuestGen.slate.TryGet<Map>(storeAs.GetValue(slate), out var var) || !IsAcceptableMap(var, slate)) && TryFindMap(slate, out var))
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), var);
			}
		}

		private bool TryFindMap(Slate slate, out Map map)
		{
			if (!preferMapWithMinFreeColonists.TryGetValue(slate, out var minCount))
			{
				minCount = 1;
			}
			IEnumerable<Map> source = Find.Maps.Where((Map x) => x.IsPlayerHome && IsAcceptableMap(x, slate));
			if (!source.Where((Map x) => x.mapPawns.FreeColonists.Count >= minCount).TryRandomElement(out map))
			{
				return source.Where((Map x) => x.mapPawns.FreeColonists.Any()).TryRandomElement(out map);
			}
			return true;
		}

		private bool IsAcceptableMap(Map map, Slate slate)
		{
			if (map == null)
			{
				return false;
			}
			if (mustBeInfestable.GetValue(slate) && !InfestationCellFinder.TryFindCell(out var _, map))
			{
				return false;
			}
			return true;
		}
	}
}
