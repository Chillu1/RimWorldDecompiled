using Verse;

namespace RimWorld;

public class TileMutatorWorker_ToxicLake : TileMutatorWorker_Lake
{
	public TileMutatorWorker_ToxicLake(TileMutatorDef def)
		: base(def)
	{
	}

	protected override void ProcessCell(IntVec3 cell, Map map)
	{
		float valAt = GetValAt(cell, map);
		if (GenerateDeepWater && valAt > 0.75f)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.ToxicWaterDeep);
		}
		else if (valAt > 0.5f)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.ToxicWaterShallow);
		}
		else if (valAt > 0.45f && MapGenUtility.ShouldGenerateBeachSand(cell, map))
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.LakeshoreTerrainAt(cell, map));
		}
	}
}
