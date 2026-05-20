using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public static class QuestGen_Get
{
	public static Map GetMap(bool mustBeInfestable = false, int? preferMapWithMinFreeColonists = null, bool canBeSpace = false)
	{
		int minCount = preferMapWithMinFreeColonists ?? 1;
		List<Map> source = Find.Maps.Where(Validator).ToList();
		if (!source.Where((Map x) => x.mapPawns.FreeColonists.Count >= minCount).TryRandomElement(out var result))
		{
			source.Where((Map x) => x.mapPawns.FreeColonists.Any()).TryRandomElement(out result);
		}
		if (result == null && !mustBeInfestable && canBeSpace)
		{
			return Find.Maps.FirstOrDefault((Map x) => x.IsPlayerHome);
		}
		return result;
		bool Validator(Map m)
		{
			if (m.IsPlayerHome && (!mustBeInfestable || InfestationCellFinder.TryFindCell(out var _, m)))
			{
				if (!canBeSpace)
				{
					return !m.Tile.LayerDef.isSpace;
				}
				return true;
			}
			return false;
		}
	}
}
