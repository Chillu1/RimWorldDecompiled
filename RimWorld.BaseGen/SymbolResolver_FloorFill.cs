using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_FloorFill : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			TerrainGrid terrainGrid = map.terrainGrid;
			TerrainDef terrainDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
			bool flag = rp.floorOnlyIfTerrainSupports ?? false;
			bool flag2 = rp.allowBridgeOnAnyImpassableTerrain ?? false;
			foreach (IntVec3 item in rp.rect)
			{
				if ((!rp.chanceToSkipFloor.HasValue || !Rand.Chance(rp.chanceToSkipFloor.Value)) && (!flag || GenConstruct.CanBuildOnTerrain(terrainDef, item, map, Rot4.North) || (flag2 && item.GetTerrain(map).passability == Traversability.Impassable)))
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
}
