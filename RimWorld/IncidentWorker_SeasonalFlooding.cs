using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentWorker_SeasonalFlooding : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		GenSpawn.Spawn(ThingDefOf.SeasonalFlood, map.Center, map);
		return true;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!(map.TileInfo is SurfaceTile surfaceTile))
		{
			return false;
		}
		if (surfaceTile.Rivers.NullOrEmpty())
		{
			return false;
		}
		if (map.mapTemperature.OutdoorTemp < -7f)
		{
			return false;
		}
		Vector2 longLat = Find.WorldGrid.LongLatOf(Find.WorldSelector.SelectedTile);
		Season season = GenDate.Season(Find.TickManager.TicksGame, longLat);
		if (season != Season.Spring && season != Season.PermanentSummer)
		{
			return false;
		}
		return true;
	}
}
