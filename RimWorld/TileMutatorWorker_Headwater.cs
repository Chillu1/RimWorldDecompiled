using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Headwater : TileMutatorWorker_River
{
	private ModuleBase lakeNoise;

	private const float LakeRadius = 0.35f;

	private const float MacroNoiseStrength = 15f;

	public TileMutatorWorker_Headwater(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		return def.label;
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			lakeNoise = new DistFromPoint((float)map.Size.x * 0.35f);
			lakeNoise = new ScaleBias(-1.0, 1.0, lakeNoise);
			lakeNoise = new Scale(TileMutatorWorker_Lake.LakeSquashRange.RandomInRange, 1.0, 1.0, lakeNoise);
			lakeNoise = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, lakeNoise);
			lakeNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, lakeNoise);
			lakeNoise = MapNoiseUtility.AddDisplacementNoise(lakeNoise, 0.006f, 15f, 2);
			lakeNoise = MapNoiseUtility.AddDisplacementNoise(lakeNoise, 0.015f, 15f);
		}
	}

	protected override IntVec3 GetRiverCenter(Map map)
	{
		return map.Center;
	}

	protected override void GenerateRiverGraph(Map map)
	{
		if (!map.TileInfo.Isnt<SurfaceTile>(out var casted))
		{
			SurfaceTile.RiverLink riverLink = casted.Rivers.First();
			float headingFromTo = Find.WorldGrid.GetHeadingFromTo(casted.tile, riverLink.neighbor.Tile.tile);
			var (vector, vector2) = GetMapEdgeNodes(map, headingFromTo);
			if (IsFlowingAToB(vector, vector2, headingFromTo))
			{
				RiverNode item = new RiverNode
				{
					start = riverCenter.ToVector3Shifted(),
					end = vector2,
					width = riverLink.river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item);
			}
			else
			{
				RiverNode item2 = new RiverNode
				{
					start = riverCenter.ToVector3Shifted(),
					end = vector,
					width = riverLink.river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item2);
			}
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		base.GeneratePostElevationFertility(map);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (GetValAt(allCell) > 0.45f)
			{
				elevation[allCell] = 0f;
			}
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		base.GeneratePostTerrain(map);
		foreach (IntVec3 allCell in map.AllCells)
		{
			ProcessCell(allCell, map);
		}
	}

	private void ProcessCell(IntVec3 cell, Map map)
	{
		float valAt = GetValAt(cell);
		if (valAt > 0.75f)
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.DeepFreshWaterTerrainAt(cell, map));
		}
		else if (valAt > 0.5f)
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.ShallowFreshWaterTerrainAt(cell, map));
		}
		else if (valAt > 0.45f && MapGenUtility.ShouldGenerateBeachSand(cell, map))
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.LakeshoreTerrainAt(cell, map));
		}
	}

	private float GetValAt(IntVec3 cell)
	{
		return (float)lakeNoise.GetValue(cell.x, 0.0, cell.z);
	}
}
