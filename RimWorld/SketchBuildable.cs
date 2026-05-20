using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class SketchBuildable : SketchEntity
{
	public abstract BuildableDef Buildable { get; }

	public abstract ThingDef Stuff { get; }

	public override string Label => GenLabel.ThingLabel(Buildable, Stuff);

	public override bool LostImportantReferences => Buildable == null;

	public abstract Thing GetSpawnedBlueprintOrFrame(IntVec3 at, Map map);

	public override bool IsSameSpawnedOrBlueprintOrFrame(IntVec3 at, Map map)
	{
		if (!at.InBounds(map))
		{
			return false;
		}
		if (IsSameSpawned(at, map))
		{
			return true;
		}
		return GetSpawnedBlueprintOrFrame(at, map) != null;
	}

	public Thing FirstPermanentBlockerAt(IntVec3 at, Map map)
	{
		foreach (IntVec3 item in GenAdj.OccupiedRect(at, Rot4.North, Buildable.Size))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!thingList[i].def.destroyable && !GenConstruct.CanPlaceBlueprintOver(Buildable, thingList[i].def))
				{
					return thingList[i];
				}
			}
		}
		return null;
	}

	protected void ForceTerrainAffordance(IntVec3 at, Rot4 rot, Map map, TerrainDef defaultTerrain)
	{
		TerrainDef newTerr = ((defaultTerrain == null || !defaultTerrain.affordances.Contains(Buildable.terrainAffordanceNeeded)) ? map.Biome.TerrainForAffordance(Buildable.terrainAffordanceNeeded) : defaultTerrain);
		CellRect cellRect = GenAdj.OccupiedRect(at, rot, Buildable.Size);
		cellRect.ClipInsideMap(map);
		foreach (IntVec3 item in cellRect)
		{
			if (map.terrainGrid.TerrainAt(item).IsWater)
			{
				map.terrainGrid.SetFoundation(item, ModsConfig.OdysseyActive ? TerrainDefOf.HeavyBridge : TerrainDefOf.Bridge);
			}
			else
			{
				map.terrainGrid.SetTerrain(item, newTerr);
			}
		}
	}
}
