using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Caves : TileMutatorWorker
{
	protected ModuleBase directionNoise;

	public const float DirectionNoiseFrequency = 0.00205f;

	private const int AllowBranchingAfterThisManyCells = 10;

	private const float WaterFrequency = 0.08f;

	private const float GravelFrequency = 0.16f;

	private const float WaterThreshold = 0.93f;

	private const float GravelThreshold = 0.55f;

	protected virtual float BranchChance => 0.02f;

	protected virtual float WidthOffsetPerCell => 0.034f;

	protected virtual float MinTunnelWidth => 1.4f;

	public TileMutatorWorker_Caves(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		directionNoise = new Perlin(0.002050000010058284, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		GenerateCaves(map);
	}

	protected void GenerateCaves(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		BoolGrid visited = new BoolGrid(map);
		List<IntVec3> list = new List<IntVec3>();
		MapGenCavesUtility.CaveGenParms parms = MapGenCavesUtility.CaveGenParms.Default;
		parms.widthOffsetPerCell = WidthOffsetPerCell;
		parms.minTunnelWidth = MinTunnelWidth;
		parms.branchChance = BranchChance;
		parms.allowBranchingAfterThisManyCells = 10;
		MapGenCavesUtility.GenerateCaves(map, visited, list, directionNoise, parms, (IntVec3 cell) => ShouldCarve(cell, elevation, map));
	}

	public override void GeneratePostTerrain(Map map)
	{
		Perlin perlin = new Perlin(0.07999999821186066, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
		Perlin perlin2 = new Perlin(0.1599999964237213, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
		MapGenFloatGrid caves = MapGenerator.Caves;
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (!(caves[allCell] <= 0f) && !(elevation[allCell] <= 0f) && !allCell.GetTerrain(map).IsRiver)
			{
				float num = (float)perlin.GetValue(allCell.x, 0.0, allCell.z);
				float num2 = (float)perlin2.GetValue(allCell.x, 0.0, allCell.z);
				if (num > 0.93f)
				{
					map.terrainGrid.SetTerrain(allCell, MapGenUtility.ShallowFreshWaterTerrainAt(allCell, map));
				}
				else if (num2 > 0.55f)
				{
					map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Gravel);
				}
			}
		}
	}

	protected virtual bool ShouldCarve(IntVec3 c, MapGenFloatGrid elevation, Map map)
	{
		return IsRock(c, elevation, map);
	}

	public static bool IsRock(IntVec3 c, MapGenFloatGrid elevation, Map map)
	{
		if (c.InBounds(map))
		{
			return elevation[c] > 0.7f;
		}
		return false;
	}
}
