using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_Terrain : GenStep
	{
		private struct GRLT_Entry
		{
			public float bestDistance;

			public IntVec3 bestNode;
		}

		private static bool debug_WarnedMissingTerrain;

		public override int SeedPart => 262606459;

		public override void Generate(Map map, GenStepParams parms)
		{
			BeachMaker.Init(map);
			RiverMaker riverMaker = GenerateRiver(map);
			List<IntVec3> list = new List<IntVec3>();
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid fertility = MapGenerator.Fertility;
			MapGenFloatGrid caves = MapGenerator.Caves;
			TerrainGrid terrainGrid = map.terrainGrid;
			foreach (IntVec3 allCell in map.AllCells)
			{
				Building edifice = allCell.GetEdifice(map);
				TerrainDef terrainDef = null;
				terrainDef = (((edifice == null || edifice.def.Fillage != FillCategory.Full) && !(caves[allCell] > 0f)) ? TerrainFrom(allCell, map, elevation[allCell], fertility[allCell], riverMaker, preferSolid: false) : TerrainFrom(allCell, map, elevation[allCell], fertility[allCell], riverMaker, preferSolid: true));
				if (terrainDef.IsRiver && edifice != null)
				{
					list.Add(edifice.Position);
					edifice.Destroy();
				}
				terrainGrid.SetTerrain(allCell, terrainDef);
			}
			riverMaker?.ValidatePassage(map);
			RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
			BeachMaker.Cleanup();
			foreach (TerrainPatchMaker terrainPatchMaker in map.Biome.terrainPatchMakers)
			{
				terrainPatchMaker.Cleanup();
			}
		}

		private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, RiverMaker river, bool preferSolid)
		{
			TerrainDef terrainDef = null;
			if (river != null)
			{
				terrainDef = river.TerrainAt(c, recordForValidation: true);
			}
			if (terrainDef == null && preferSolid)
			{
				return GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
			}
			TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, map.Biome);
			if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
			{
				return terrainDef2;
			}
			if (terrainDef != null && terrainDef.IsRiver)
			{
				return terrainDef;
			}
			if (terrainDef2 != null)
			{
				return terrainDef2;
			}
			if (terrainDef != null)
			{
				return terrainDef;
			}
			for (int i = 0; i < map.Biome.terrainPatchMakers.Count; i++)
			{
				terrainDef2 = map.Biome.terrainPatchMakers[i].TerrainAt(c, map, fertility);
				if (terrainDef2 != null)
				{
					return terrainDef2;
				}
			}
			if (elevation > 0.55f && elevation < 0.61f)
			{
				return TerrainDefOf.Gravel;
			}
			if (elevation >= 0.61f)
			{
				return GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
			}
			terrainDef2 = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility);
			if (terrainDef2 != null)
			{
				return terrainDef2;
			}
			if (!debug_WarnedMissingTerrain)
			{
				Log.Error("No terrain found in biome " + map.Biome.defName + " for elevation=" + elevation + ", fertility=" + fertility);
				debug_WarnedMissingTerrain = true;
			}
			return TerrainDefOf.Sand;
		}

		private RiverMaker GenerateRiver(Map map)
		{
			List<Tile.RiverLink> rivers = Find.WorldGrid[map.Tile].Rivers;
			if (rivers == null || rivers.Count == 0)
			{
				return null;
			}
			float angle = Find.WorldGrid.GetHeadingFromTo(map.Tile, rivers.OrderBy((Tile.RiverLink rl) => -rl.river.degradeThreshold).First().neighbor);
			Rot4 a = Find.World.CoastDirectionAt(map.Tile);
			if (a != Rot4.Invalid)
			{
				angle = a.AsAngle + (float)Rand.RangeInclusive(-30, 30);
			}
			RiverMaker riverMaker = new RiverMaker(new Vector3(Rand.Range(0.3f, 0.7f) * (float)map.Size.x, 0f, Rand.Range(0.3f, 0.7f) * (float)map.Size.z), angle, rivers.OrderBy((Tile.RiverLink rl) => -rl.river.degradeThreshold).FirstOrDefault().river);
			GenerateRiverLookupTexture(map, riverMaker);
			return riverMaker;
		}

		private void UpdateRiverAnchorEntry(Dictionary<int, GRLT_Entry> entries, IntVec3 center, int entryId, float zValue)
		{
			float num = zValue - (float)entryId;
			if (!(num > 2f) && (!entries.ContainsKey(entryId) || entries[entryId].bestDistance > num))
			{
				entries[entryId] = new GRLT_Entry
				{
					bestDistance = num,
					bestNode = center
				};
			}
		}

		private void GenerateRiverLookupTexture(Map map, RiverMaker riverMaker)
		{
			int num = Mathf.CeilToInt(DefDatabase<RiverDef>.AllDefs.Select((RiverDef rd) => rd.widthOnMap / 2f + 8f).Max());
			int num2 = Mathf.Max(4, num) * 2;
			Dictionary<int, GRLT_Entry> dictionary = new Dictionary<int, GRLT_Entry>();
			Dictionary<int, GRLT_Entry> dictionary2 = new Dictionary<int, GRLT_Entry>();
			Dictionary<int, GRLT_Entry> dictionary3 = new Dictionary<int, GRLT_Entry>();
			for (int i = -num2; i < map.Size.z + num2; i++)
			{
				for (int j = -num2; j < map.Size.x + num2; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					Vector3 vector = riverMaker.WaterCoordinateAt(intVec);
					int entryId = Mathf.FloorToInt(vector.z / 4f);
					UpdateRiverAnchorEntry(dictionary, intVec, entryId, (vector.z + Mathf.Abs(vector.x)) / 4f);
					UpdateRiverAnchorEntry(dictionary2, intVec, entryId, (vector.z + Mathf.Abs(vector.x - (float)num)) / 4f);
					UpdateRiverAnchorEntry(dictionary3, intVec, entryId, (vector.z + Mathf.Abs(vector.x + (float)num)) / 4f);
				}
			}
			int num3 = Mathf.Max(dictionary.Keys.Min(), dictionary2.Keys.Min(), dictionary3.Keys.Min());
			int num4 = Mathf.Min(dictionary.Keys.Max(), dictionary2.Keys.Max(), dictionary3.Keys.Max());
			for (int k = num3; k < num4; k++)
			{
				WaterInfo waterInfo = map.waterInfo;
				if (dictionary2.ContainsKey(k) && dictionary2.ContainsKey(k + 1))
				{
					waterInfo.riverDebugData.Add(dictionary2[k].bestNode.ToVector3Shifted());
					waterInfo.riverDebugData.Add(dictionary2[k + 1].bestNode.ToVector3Shifted());
				}
				if (dictionary.ContainsKey(k) && dictionary.ContainsKey(k + 1))
				{
					waterInfo.riverDebugData.Add(dictionary[k].bestNode.ToVector3Shifted());
					waterInfo.riverDebugData.Add(dictionary[k + 1].bestNode.ToVector3Shifted());
				}
				if (dictionary3.ContainsKey(k) && dictionary3.ContainsKey(k + 1))
				{
					waterInfo.riverDebugData.Add(dictionary3[k].bestNode.ToVector3Shifted());
					waterInfo.riverDebugData.Add(dictionary3[k + 1].bestNode.ToVector3Shifted());
				}
				if (dictionary2.ContainsKey(k) && dictionary.ContainsKey(k))
				{
					waterInfo.riverDebugData.Add(dictionary2[k].bestNode.ToVector3Shifted());
					waterInfo.riverDebugData.Add(dictionary[k].bestNode.ToVector3Shifted());
				}
				if (dictionary.ContainsKey(k) && dictionary3.ContainsKey(k))
				{
					waterInfo.riverDebugData.Add(dictionary[k].bestNode.ToVector3Shifted());
					waterInfo.riverDebugData.Add(dictionary3[k].bestNode.ToVector3Shifted());
				}
			}
			CellRect cellRect = new CellRect(-2, -2, map.Size.x + 4, map.Size.z + 4);
			float[] array = new float[cellRect.Area * 2];
			int num5 = 0;
			for (int l = cellRect.minZ; l <= cellRect.maxZ; l++)
			{
				for (int m = cellRect.minX; m <= cellRect.maxX; m++)
				{
					IntVec3 a = new IntVec3(m, 0, l);
					bool flag = true;
					for (int n = 0; n < GenAdj.AdjacentCellsAndInside.Length; n++)
					{
						if (riverMaker.TerrainAt(a + GenAdj.AdjacentCellsAndInside[n]) != null)
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						Vector2 p = a.ToIntVec2.ToVector2();
						int num6 = int.MinValue;
						Vector2 vector2 = Vector2.zero;
						for (int num7 = num3; num7 < num4; num7++)
						{
							if (dictionary2.ContainsKey(num7) && dictionary2.ContainsKey(num7 + 1) && dictionary.ContainsKey(num7) && dictionary.ContainsKey(num7 + 1) && dictionary3.ContainsKey(num7) && dictionary3.ContainsKey(num7 + 1))
							{
								Vector2 p2 = dictionary2[num7].bestNode.ToIntVec2.ToVector2();
								Vector2 p3 = dictionary2[num7 + 1].bestNode.ToIntVec2.ToVector2();
								Vector2 p4 = dictionary[num7].bestNode.ToIntVec2.ToVector2();
								Vector2 p5 = dictionary[num7 + 1].bestNode.ToIntVec2.ToVector2();
								Vector2 p6 = dictionary3[num7].bestNode.ToIntVec2.ToVector2();
								Vector2 p7 = dictionary3[num7 + 1].bestNode.ToIntVec2.ToVector2();
								Vector2 vector3 = GenGeo.InverseQuadBilinear(p, p4, p2, p5, p3);
								if (vector3.x >= -0.0001f && vector3.x <= 1.0001f && vector3.y >= -0.0001f && vector3.y <= 1.0001f)
								{
									vector2 = new Vector2((0f - vector3.x) * (float)num, (vector3.y + (float)num7) * 4f);
									num6 = num7;
									break;
								}
								Vector2 vector4 = GenGeo.InverseQuadBilinear(p, p4, p6, p5, p7);
								if (vector4.x >= -0.0001f && vector4.x <= 1.0001f && vector4.y >= -0.0001f && vector4.y <= 1.0001f)
								{
									vector2 = new Vector2(vector4.x * (float)num, (vector4.y + (float)num7) * 4f);
									num6 = num7;
									break;
								}
							}
						}
						if (num6 == int.MinValue)
						{
							Log.ErrorOnce("Failed to find all necessary river flow data", 5273133);
						}
						array[num5] = vector2.x;
						array[num5 + 1] = vector2.y;
					}
					num5 += 2;
				}
			}
			float[] array2 = new float[cellRect.Area * 2];
			float[] array3 = new float[9]
			{
				0.123317f,
				0.123317f,
				0.123317f,
				0.123317f,
				0.077847f,
				0.077847f,
				0.077847f,
				0.077847f,
				0.195346f
			};
			int num8 = 0;
			for (int num9 = cellRect.minZ; num9 <= cellRect.maxZ; num9++)
			{
				for (int num10 = cellRect.minX; num10 <= cellRect.maxX; num10++)
				{
					IntVec3 a2 = new IntVec3(num10, 0, num9);
					float num11 = 0f;
					float num12 = 0f;
					float num13 = 0f;
					for (int num14 = 0; num14 < GenAdj.AdjacentCellsAndInside.Length; num14++)
					{
						IntVec3 c = a2 + GenAdj.AdjacentCellsAndInside[num14];
						if (cellRect.Contains(c))
						{
							int num15 = num8 + (GenAdj.AdjacentCellsAndInside[num14].x + GenAdj.AdjacentCellsAndInside[num14].z * cellRect.Width) * 2;
							if (array[num15] != 0f || array[num15 + 1] != 0f)
							{
								num11 += array[num15] * array3[num14];
								num12 += array[num15 + 1] * array3[num14];
								num13 += array3[num14];
							}
						}
					}
					if (num13 > 0f)
					{
						array2[num8] = num11 / num13;
						array2[num8 + 1] = num12 / num13;
					}
					num8 += 2;
				}
			}
			array = array2;
			for (int num16 = 0; num16 < array.Length; num16 += 2)
			{
				if (array[num16] != 0f || array[num16 + 1] != 0f)
				{
					Vector2 vector5 = Rand.InsideUnitCircle * 0.4f;
					array[num16] += vector5.x;
					array[num16 + 1] += vector5.y;
				}
			}
			byte[] array4 = new byte[array.Length * 4];
			Buffer.BlockCopy(array, 0, array4, 0, array.Length * 4);
			map.waterInfo.riverOffsetMap = array4;
			map.waterInfo.GenerateRiverFlowMap();
		}
	}
}
