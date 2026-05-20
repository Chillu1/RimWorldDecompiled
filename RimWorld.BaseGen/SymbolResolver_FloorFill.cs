using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_FloorFill : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		TerrainGrid terrainGrid = map.terrainGrid;
		TerrainDef terrainDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
		bool valueOrDefault = rp.floorOnlyIfTerrainSupports == true;
		bool valueOrDefault2 = rp.allowBridgeOnAnyImpassableTerrain == true;
		foreach (IntVec3 item in rp.rect)
		{
			if ((!rp.chanceToSkipFloor.HasValue || !Rand.Chance(rp.chanceToSkipFloor.Value)) && (!valueOrDefault || GenConstruct.CanBuildOnTerrain(terrainDef, item, map, Rot4.North) || (valueOrDefault2 && item.GetTerrain(map).passability == Traversability.Impassable)))
			{
				terrainGrid.SetTerrain(item, terrainDef);
				if (rp.filthDef != null)
				{
					FilthMaker.TryMakeFilth(item, map, rp.filthDef, (!rp.filthDensity.HasValue) ? 1 : Mathf.RoundToInt(rp.filthDensity.Value.RandomInRange));
				}
			}
		}
	}
}
