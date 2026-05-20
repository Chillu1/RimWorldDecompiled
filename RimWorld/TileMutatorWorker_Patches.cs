using System.Linq;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_Patches : TileMutatorWorker
{
	public TileMutatorWorker_Patches(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostTerrain(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		bool flag = map.TileInfo.Mutators.Any((TileMutatorDef m) => m.preventsPondGeneration);
		if (map.TileInfo.Mutators.Any((TileMutatorDef m) => m.preventPatches))
		{
			return;
		}
		foreach (IntVec3 allCell in map.AllCells)
		{
			TerrainDef terrain = allCell.GetTerrain(map);
			if (terrain.categoryType == TerrainDef.TerrainCategoryType.Stone || terrain.IsWater || terrain == MapGenUtility.BeachTerrainAt(allCell, map) || terrain == MapGenUtility.LakeshoreTerrainAt(allCell, map))
			{
				continue;
			}
			foreach (TerrainPatchMaker terrainPatchMaker in def.terrainPatchMakers)
			{
				if (!(terrainPatchMaker.isPond && flag))
				{
					TerrainDef terrainDef = terrainPatchMaker.TerrainAt(allCell, map, allCell.GetFertility(map));
					if (terrainDef != null)
					{
						map.terrainGrid.SetTerrain(allCell, terrainDef);
					}
				}
			}
		}
		foreach (TerrainPatchMaker terrainPatchMaker2 in def.terrainPatchMakers)
		{
			terrainPatchMaker2.Cleanup();
		}
	}
}
