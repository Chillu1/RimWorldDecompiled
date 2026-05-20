using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class GetOrGenerateMapUtility
{
	public static Map GetOrGenerateMap(PlanetTile tile, IntVec3 size, WorldObjectDef suggestedMapParentDef, IEnumerable<GenStepWithParams> extraGenStepDefs = null, bool stepDebugger = false)
	{
		Map map = Current.Game.FindMap(tile);
		if (map == null)
		{
			MapParent mapParent = Find.WorldObjects.MapParentAt(tile);
			if (mapParent == null)
			{
				if (suggestedMapParentDef == null)
				{
					Log.Error($"Tried to get or generate map at {tile}, but there isn't any MapParent world object here and map parent def argument is null.");
					return null;
				}
				mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(suggestedMapParentDef);
				mapParent.Tile = tile;
				Find.WorldObjects.Add(mapParent);
			}
			map = (tile.Valid ? MapGenerator.GenerateMap(size, mapParent, mapParent.MapGeneratorDef, mapParent.ExtraGenStepDefs.ConcatIfNotNull(extraGenStepDefs), null, isPocketMap: false, stepDebugger) : PocketMapUtility.GeneratePocketMap(size, mapParent.MapGeneratorDef, extraGenStepDefs, Find.AnyPlayerHomeMap));
		}
		return map;
	}

	public static Map GetOrGenerateMap(PlanetTile tile, WorldObjectDef suggestedMapParentDef, IEnumerable<GenStepWithParams> extraGenStepDefs = null)
	{
		return GetOrGenerateMap(tile, Find.World.info.initialMapSize, suggestedMapParentDef, extraGenStepDefs);
	}
}
