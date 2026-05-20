using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_Terrain : GenStep
{
	public override int SeedPart => 262606459;

	public override void Generate(Map map, GenStepParams parms)
	{
		TerrainGrid terrainGrid = map.terrainGrid;
		List<IntVec3> list = new List<IntVec3>();
		using (map.pathing.DisableIncrementalScope())
		{
			foreach (IntVec3 allCell in map.AllCells)
			{
				Building edifice = allCell.GetEdifice(map);
				TerrainDef naturalTerrainAt = MapGenUtility.GetNaturalTerrainAt(allCell, map);
				if (!naturalTerrainAt.supportsRock && edifice != null)
				{
					list.Add(edifice.Position);
					edifice.Destroy();
				}
				terrainGrid.SetTerrain(allCell, naturalTerrainAt);
			}
			RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
			foreach (TerrainPatchMaker terrainPatchMaker in map.Biome.terrainPatchMakers)
			{
				terrainPatchMaker.Cleanup();
			}
		}
	}
}
