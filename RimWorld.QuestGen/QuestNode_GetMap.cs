using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetMap : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs = "map";

	public SlateRef<bool> mustBeInfestable;

	public SlateRef<bool> canBeSpace;

	public SlateRef<int> preferMapWithMinFreeColonists;

	public SlateRef<List<PlanetLayerDef>> layerWhitelist;

	public SlateRef<List<PlanetLayerDef>> layerBlacklist;

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
		if (!preferMapWithMinFreeColonists.TryGetValue(slate, out var minCount) || minCount < 1)
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
		if (map.IsPocketMap)
		{
			return false;
		}
		if (!canBeSpace.GetValue(slate) && map.Tile.Valid && map.Tile.LayerDef.isSpace)
		{
			return false;
		}
		if (mustBeInfestable.GetValue(slate) && !InfestationCellFinder.TryFindCell(out var _, map))
		{
			return false;
		}
		List<PlanetLayerDef> value = layerWhitelist.GetValue(slate);
		List<PlanetLayerDef> value2 = layerBlacklist.GetValue(slate);
		if (!value.NullOrEmpty() && map.Tile.Valid && !value.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		if (!value2.NullOrEmpty() && map.Tile.Valid && value2.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		return true;
	}
}
