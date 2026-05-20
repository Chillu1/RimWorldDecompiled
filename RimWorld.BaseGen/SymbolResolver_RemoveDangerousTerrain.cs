using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_RemoveDangerousTerrain : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		TerrainGrid terrainGrid = map.terrainGrid;
		foreach (IntVec3 item in rp.rect)
		{
			if (terrainGrid.TerrainAt(item).dangerous)
			{
				terrainGrid.SetTerrain(item, map.BiomeAt(item).TerrainForAffordance(TerrainAffordanceDefOf.Heavy));
			}
		}
	}
}
