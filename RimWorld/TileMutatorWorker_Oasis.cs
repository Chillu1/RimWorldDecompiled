using LudeonTK;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_Oasis : TileMutatorWorker_Lake
{
	private new const float DeepWaterThreshold = 0.75f;

	private new const float WaterThreshold = 0.57f;

	private const float RichSoilThreshold = 0.45f;

	private const float SoilThreshold = 0.3f;

	[TweakValue("Oasis", 0f, 1f)]
	private static float LakeRadiusInt = 0.4f;

	[TweakValue("Oasis", 0f, 5f)]
	private static float LakeFalloffDecayInt = 2f;

	protected override float LakeRadius => LakeRadiusInt;

	protected override float LakeFalloffDecay => LakeFalloffDecayInt;

	public TileMutatorWorker_Oasis(TileMutatorDef def)
		: base(def)
	{
	}

	protected override IntVec3 GetLakeCenter(Map map)
	{
		return CellFinder.RandomNotEdgeCell(map.Size.x / 2, map);
	}

	protected override void ProcessCell(IntVec3 cell, Map map)
	{
		float valAt = GetValAt(cell, map);
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
		if (valAt > 0.75f)
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.DeepFreshWaterTerrainAt(cell, map));
		}
		else if (valAt > 0.57f)
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.ShallowFreshWaterTerrainAt(cell, map));
		}
		else if (valAt > 0.45f && !terrainDef.IsWater)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.SoilRich);
		}
		else if (valAt > 0.3f && !terrainDef.IsWater)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
		}
	}
}
