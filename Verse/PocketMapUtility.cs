using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class PocketMapUtility
{
	public static MapPortal currentlyGeneratingPortal;

	public static Map GeneratePocketMap(IntVec3 size, MapGeneratorDef generatorDef, IEnumerable<GenStepWithParams> extraGenStepDefs, Map sourceMap)
	{
		PocketMapParent pocketMapParent = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) as PocketMapParent;
		pocketMapParent.sourceMap = sourceMap;
		pocketMapParent.mapGenerator = generatorDef;
		Map result = MapGenerator.GenerateMap(size, pocketMapParent, generatorDef, extraGenStepDefs, null, isPocketMap: true);
		Find.World.pocketMaps.Add(pocketMapParent);
		return result;
	}

	public static void DestroyPocketMap(Map map)
	{
		PocketMapParent pocketMapParent = map?.Parent as PocketMapParent;
		if (map != null && pocketMapParent != null)
		{
			Find.World.pocketMaps.Remove(pocketMapParent);
			Current.Game.DeinitAndRemoveMap(map, notifyPlayer: true);
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}
