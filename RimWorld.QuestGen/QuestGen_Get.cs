using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public static class QuestGen_Get
	{
		public static Map GetMap(bool mustBeInfestable = false, int? preferMapWithMinFreeColonists = null)
		{
			int minCount = preferMapWithMinFreeColonists ?? 1;
			IntVec3 cell;
			IEnumerable<Map> source = Find.Maps.Where((Map x) => x.IsPlayerHome && x != null && (!mustBeInfestable || InfestationCellFinder.TryFindCell(out cell, x)));
			if (!source.Where((Map x) => x.mapPawns.FreeColonists.Count >= minCount).TryRandomElement(out var result))
			{
				source.Where((Map x) => x.mapPawns.FreeColonists.Any()).TryRandomElement(out result);
			}
			return result;
		}
	}
}
