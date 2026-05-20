using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_River : TileMutatorWorker
{
	private const float RiverCurveFrequency = 0.007f;

	private const float RiverCurveLacunarity = 2f;

	private const float RiverCurvePersistence = 1f;

	private const int RiverCurveOctaves = 2;

	private const float RiverCurveAmplitude = 40f;

	private const float RiverWidthFrequency = 0.02f;

	private const float RiverWidthLacunarity = 2f;

	private const float RiverWidthPersistence = 1f;

	private const int RiverWidthOctaves = 3;

	protected const float RiverWidthNoiseAmplitude = 0.15f;

	private const float RemoveRoofWidthThreshold = 20f;

	private const float RiverCaveBankFactor = 0.3f;

	private const float ShallowFactor = 0.2f;

	private const int Oversample = 25;

	protected ModuleBase riverBendNoise;

	protected ModuleBase riverWidthNoise;

	protected ModuleBase shallowizer;

	private ModuleBase riverbankNoise;

	protected IntVec3 riverCenter;

	private Dictionary<RiverNode, float[]> nodeDepthMaps = new Dictionary<RiverNode, float[]>();

	protected virtual float GetCurveFrequency => 0.007f;

	protected virtual float GetCurveAmplitude => 40f;

	public TileMutatorWorker_River(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		if (tile.Tile is SurfaceTile surfaceTile)
		{
			return surfaceTile.Rivers[0].river.label;
		}
		throw new Exception("Attempted to get river label on a tile which is not a SurfaceTile");
	}

	public override void Init(Map map)
	{
		if (map.waterInfo.lakeCenter.IsValid)
		{
			riverCenter = map.waterInfo.lakeCenter;
		}
		else
		{
			riverCenter = GetRiverCenter(map);
		}
		riverBendNoise = new Perlin(GetCurveFrequency, 2.0, 1.0, 2, Rand.Int, QualityMode.Medium);
		shallowizer = new Perlin(0.029999999329447746, 2.0, 0.5, 3, Rand.Int, QualityMode.Medium);
		shallowizer = new Abs(shallowizer);
		riverbankNoise = new Perlin(0.029999999329447746, 2.0, 2.0, 2, Rand.Int, QualityMode.Medium);
		riverWidthNoise = new Perlin(0.019999999552965164, 2.0, 1.0, 3, Rand.Int, QualityMode.Medium);
	}

	public override void GeneratePostTerrain(Map map)
	{
		GenerateRiverGraph(map);
		if (map.waterInfo.riverGraph.NullOrEmpty())
		{
			return;
		}
		GenerateDepthMaps(map);
		List<IntVec3> list = new List<IntVec3>();
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			Building edifice = allCell.GetEdifice(map);
			TerrainDef terrainDef = RiverTerrainAt(allCell, map);
			TerrainDef terrainDef2 = RiverBankTerrainAt(allCell, map);
			float riverWidth;
			float depth = GetDepth(allCell, map, out riverWidth);
			float value = riverbankNoise.GetValue(allCell);
			float num = (float)map.Biome.riverbankSizeRange.Lerped(value) * 0.3f;
			bool flag = riverWidth > 20f;
			if (depth > 0f - num)
			{
				edifice?.Destroy();
				if (flag)
				{
					map.roofGrid.SetRoof(allCell, null);
				}
				else if (edifice != null)
				{
					list.Add(edifice.Position);
				}
			}
			if (terrainDef != null)
			{
				map.terrainGrid.SetTerrain(allCell, terrainDef);
				elevation[allCell] = -1f;
			}
			else if (terrainDef2 != null && edifice == null)
			{
				map.terrainGrid.SetTerrain(allCell, terrainDef2);
			}
		}
		RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
	}

	public override void GeneratePostFog(Map map)
	{
		if (!map.waterInfo.riverGraph.NullOrEmpty())
		{
			GenerateRiverLookupTexture(map);
		}
	}

	protected virtual void GenerateRiverGraph(Map map)
	{
		if (!map.TileInfo.Isnt<SurfaceTile>(out var casted))
		{
			List<SurfaceTile.RiverLink> source = casted.Rivers.OrderBy((SurfaceTile.RiverLink rl) => -((SurfaceTile)rl.neighbor.Tile).riverDist).ToList();
			float headingFromTo = Find.WorldGrid.GetHeadingFromTo(source.First().neighbor.Tile.tile, source.Last().neighbor.Tile.tile);
			var (vector, vector2) = GetMapEdgeNodes(map, headingFromTo);
			if (IsFlowingAToB(vector, vector2, headingFromTo))
			{
				RiverNode item = new RiverNode
				{
					start = vector,
					end = vector2,
					width = source.First().river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item);
			}
			else
			{
				RiverNode item2 = new RiverNode
				{
					start = vector2,
					end = vector,
					width = source.First().river.widthOnMap
				};
				map.waterInfo.riverGraph.Add(item2);
			}
		}
	}

	protected virtual IntVec3 GetRiverCenter(Map map)
	{
		return new IntVec3((int)(Rand.Range(0.3f, 0.7f) * (float)map.Size.x), 0, (int)(Rand.Range(0.3f, 0.7f) * (float)map.Size.z));
	}

	protected (Vector3, Vector3) GetMapEdgeNodes(Map map, float angle)
	{
		float slope = Mathf.Tan((450f - angle) % 360f * (MathF.PI / 180f));
		List<Vector2> intersections = new List<Vector2>();
		GenGeo.LineRectIntersection(new Vector2(riverCenter.x, riverCenter.z), slope, new Vector2(-25f, -25f), new Vector2(map.Size.x + 25, map.Size.z + 25), ref intersections);
		return (new Vector3(intersections[0].x, 0f, intersections[0].y), new Vector3(intersections[1].x, 0f, intersections[1].y));
	}

	protected bool IsFlowingAToB(Vector3 a, Vector3 b, float angle)
	{
		return Mathf.RoundToInt((b - a).AngleFlat() - angle) % 360 == 0;
	}

	private void GenerateDepthMaps(Map map)
	{
		foreach (RiverNode item in map.waterInfo.riverGraph)
		{
			int num = map.Size.x + 50;
			int num2 = map.Size.z + 50;
			float[] array = new float[num * num2];
			nodeDepthMaps.Add(item, array);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					float num3 = float.MinValue;
					Vector2 vector = new Vector2(i - 25, j - 25);
					float tValue = GetTValue(item, vector);
					if (!(tValue < 0f) && !(tValue > 1f))
					{
						Vector2 displacedPoint = GetDisplacedPoint(item, tValue);
						float riverWidthAt = GetRiverWidthAt(item, vector);
						Vector2 normalized = (GetDisplacedPoint(item, tValue - 0.001f) - displacedPoint).normalized;
						float num4 = Mathf.Abs(Vector2.Dot(rhs: new Vector2(0f - normalized.y, normalized.x), lhs: vector - displacedPoint));
						num3 = riverWidthAt - num4;
					}
					array[i + j * num] = num3;
				}
			}
		}
	}

	protected virtual float GetRiverWidthAt(RiverNode riverNode, Vector2 cell)
	{
		return riverNode.width / 2f * (1f + riverWidthNoise.GetValue(cell) * 0.15f * GetWidthNoiseFactor(riverNode));
	}

	private float GetDepth(IntVec3 cell, Map map, out float riverWidth)
	{
		float num = float.MinValue;
		riverWidth = 0f;
		foreach (RiverNode item in map.waterInfo.riverGraph)
		{
			if (GetSegmentDepth(cell, map, item) > num)
			{
				num = Mathf.Max(num, GetSegmentDepth(cell, map, item));
				riverWidth = item.width;
			}
		}
		return num;
	}

	private float GetSegmentDepth(IntVec3 cell, Map map, RiverNode riverNode)
	{
		int num = cell.x + 25 + (cell.z + 25) * (map.Size.x + 50);
		return nodeDepthMaps[riverNode][num];
	}

	protected float GetTValue(RiverNode riverNode, Vector2 point)
	{
		Vector2 vector = new Vector2(riverNode.start.x, riverNode.start.z);
		Vector2 vector2 = new Vector2(riverNode.end.x, riverNode.end.z) - vector;
		Vector2 lhs = point - vector;
		float num = Vector2.Dot(vector2, vector2);
		return Vector2.Dot(lhs, vector2) / num;
	}

	protected virtual Vector2 GetDisplacedPoint(RiverNode riverNode, float t)
	{
		Vector3 vector = riverNode.end - riverNode.start;
		Vector3 vector2 = riverNode.start + t * vector;
		float num = -4f * Mathf.Pow(t, 2f) + 4f * t;
		float num2 = t * Vector3.Distance(riverNode.start, riverNode.end);
		Vector3 normalized = new Vector3(0f - vector.z, 0f, vector.x).normalized;
		float num3 = (float)riverBendNoise.GetValue(num2 * GetWidthNoiseFactor(riverNode), 0.0, riverNode.seed);
		Vector3 vector3 = vector2 + num3 * GetCurveAmplitude * normalized * num;
		return new Vector2(vector3.x, vector3.z);
	}

	protected virtual float GetWidthNoiseFactor(RiverNode riverNode)
	{
		return 6f / riverNode.width;
	}

	private TerrainDef RiverTerrainAt(IntVec3 cell, Map map)
	{
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
		if (terrainDef.IsWater && !terrainDef.IsRiver && terrainDef != TerrainDefOf.Marsh)
		{
			return null;
		}
		float riverWidth;
		float depth = GetDepth(cell, map, out riverWidth);
		if (depth > 4f && shallowizer.GetValue(cell) > 0.2f)
		{
			return MapGenUtility.DeepMovingWaterTerrainAt(cell, map);
		}
		if (depth > 0f)
		{
			return MapGenUtility.ShallowMovingWaterTerrainAt(cell, map);
		}
		return null;
	}

	private TerrainDef RiverBankTerrainAt(IntVec3 cell, Map map)
	{
		TerrainDef existing = map.terrainGrid.TerrainAt(cell);
		if (existing.IsWater && existing != TerrainDefOf.Marsh)
		{
			return null;
		}
		if (!map.Biome.terrainsByFertility.Any((TerrainThreshold tt) => tt.terrain == existing))
		{
			return null;
		}
		float riverWidth;
		float depth = GetDepth(cell, map, out riverWidth);
		float value = riverbankNoise.GetValue(cell);
		int num = map.Biome.riverbankSizeRange.Lerped(value);
		if (depth > (float)(-num))
		{
			return MapGenUtility.RiverbankTerrainAt(cell, map);
		}
		return null;
	}

	private void GenerateRiverLookupTexture(Map map)
	{
		List<IntVec3> list = new List<IntVec3>();
		List<float> list2 = new List<float>();
		List<Vector2> list3 = new List<Vector2>();
		int num = 2;
		foreach (RiverNode item2 in map.waterInfo.riverGraph)
		{
			float magnitude = (item2.end - item2.start).magnitude;
			Vector3 normalized = (item2.end - item2.start).normalized;
			Vector2 vector = (item2.start - normalized).ToVector2();
			for (int i = 0; (float)i < magnitude; i += num)
			{
				Vector2 displacedPoint = GetDisplacedPoint(item2, (float)i / magnitude);
				IntVec3 item = displacedPoint.ToVector3().ToIntVec3();
				list.Add(item);
				list2.Add(item2.width);
				list3.Add(displacedPoint - vector);
				vector = displacedPoint;
			}
		}
		map.waterInfo.riverFlowMap = new List<float>();
		for (int j = 0; j < map.Size.x * map.Size.z * 2; j++)
		{
			map.waterInfo.riverFlowMap.Add(0f);
		}
		for (int k = 0; k < map.Size.x; k++)
		{
			for (int l = 0; l < map.Size.z; l++)
			{
				IntVec3 intVec = new IntVec3(k, 0, l);
				if (!intVec.GetTerrain(map).IsRiver)
				{
					continue;
				}
				int num2 = intVec.x * map.Size.z + intVec.z;
				int num3 = 0;
				Vector2 zero = Vector2.zero;
				for (int m = 0; m < list.Count; m++)
				{
					if (!((list[m] - intVec).Magnitude > list2[m] * 1.5f / 2f + 1f))
					{
						num3++;
						zero += list3[m];
					}
				}
				zero /= (float)num3;
				map.waterInfo.riverFlowMap[num2 * 2] = zero.x;
				map.waterInfo.riverFlowMap[num2 * 2 + 1] = zero.y;
			}
		}
	}
}
