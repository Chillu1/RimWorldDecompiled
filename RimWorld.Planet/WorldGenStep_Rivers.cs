using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGenStep_Rivers : WorldGenStep
	{
		private static readonly SimpleCurve ElevationChangeCost = new SimpleCurve
		{
			new CurvePoint(-1000f, 50f),
			new CurvePoint(-100f, 100f),
			new CurvePoint(0f, 400f),
			new CurvePoint(0f, 5000f),
			new CurvePoint(100f, 50000f),
			new CurvePoint(1000f, 50000f)
		};

		private const float HillinessSmallHillsElevation = 15f;

		private const float HillinessLargeHillsElevation = 250f;

		private const float HillinessMountainousElevation = 500f;

		private const float HillinessImpassableElevation = 1000f;

		private const float NonRiverEvaporation = 0f;

		private const float EvaporationMultiple = 250f;

		public override int SeedPart => 605014749;

		public override void GenerateFresh(string seed)
		{
			GenerateRivers();
		}

		private void GenerateRivers()
		{
			Find.WorldPathGrid.RecalculateAllPerceivedPathCosts();
			List<int> coastalWaterTiles = GetCoastalWaterTiles();
			if (!coastalWaterTiles.Any())
			{
				return;
			}
			List<int> neighbors = new List<int>();
			List<int>[] array = Find.WorldPathFinder.FloodPathsWithCostForTree(coastalWaterTiles, delegate(int st, int ed)
			{
				Tile tile = Find.WorldGrid[ed];
				Tile tile2 = Find.WorldGrid[st];
				Find.WorldGrid.GetTileNeighbors(ed, neighbors);
				int num = neighbors[0];
				for (int j = 0; j < neighbors.Count; j++)
				{
					if (GetImpliedElevation(Find.WorldGrid[neighbors[j]]) < GetImpliedElevation(Find.WorldGrid[num]))
					{
						num = neighbors[j];
					}
				}
				float num2 = 1f;
				if (num != st)
				{
					num2 = 2f;
				}
				return Mathf.RoundToInt(num2 * ElevationChangeCost.Evaluate(GetImpliedElevation(tile2) - GetImpliedElevation(tile)));
			}, (int tid) => Find.WorldGrid[tid].WaterCovered);
			float[] flow = new float[array.Length];
			for (int i = 0; i < coastalWaterTiles.Count; i++)
			{
				AccumulateFlow(flow, array, coastalWaterTiles[i]);
				CreateRivers(flow, array, coastalWaterTiles[i]);
			}
		}

		private static float GetImpliedElevation(Tile tile)
		{
			float num = 0f;
			if (tile.hilliness == Hilliness.SmallHills)
			{
				num = 15f;
			}
			else if (tile.hilliness == Hilliness.LargeHills)
			{
				num = 250f;
			}
			else if (tile.hilliness == Hilliness.Mountainous)
			{
				num = 500f;
			}
			else if (tile.hilliness == Hilliness.Impassable)
			{
				num = 1000f;
			}
			return tile.elevation + num;
		}

		private List<int> GetCoastalWaterTiles()
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
			{
				if (Find.WorldGrid[i].biome != BiomeDefOf.Ocean)
				{
					continue;
				}
				Find.WorldGrid.GetTileNeighbors(i, list2);
				bool flag = false;
				for (int j = 0; j < list2.Count; j++)
				{
					if (Find.WorldGrid[list2[j]].biome != BiomeDefOf.Ocean)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list.Add(i);
				}
			}
			return list;
		}

		private void AccumulateFlow(float[] flow, List<int>[] riverPaths, int index)
		{
			Tile tile = Find.WorldGrid[index];
			flow[index] += tile.rainfall;
			if (riverPaths[index] != null)
			{
				for (int i = 0; i < riverPaths[index].Count; i++)
				{
					AccumulateFlow(flow, riverPaths, riverPaths[index][i]);
					flow[index] += flow[riverPaths[index][i]];
				}
			}
			flow[index] = Mathf.Max(0f, flow[index] - CalculateTotalEvaporation(flow[index], tile.temperature));
		}

		private void CreateRivers(float[] flow, List<int>[] riverPaths, int index)
		{
			List<int> list = new List<int>();
			Find.WorldGrid.GetTileNeighbors(index, list);
			for (int i = 0; i < list.Count; i++)
			{
				float targetFlow = flow[list[i]];
				RiverDef riverDef = DefDatabase<RiverDef>.AllDefs.Where((RiverDef rd) => rd.spawnFlowThreshold > 0 && (float)rd.spawnFlowThreshold <= targetFlow).MaxByWithFallback((RiverDef rd) => rd.spawnFlowThreshold);
				if (riverDef != null && Rand.Value < riverDef.spawnChance)
				{
					Find.WorldGrid.OverlayRiver(index, list[i], riverDef);
					ExtendRiver(flow, riverPaths, list[i], riverDef);
				}
			}
		}

		private void ExtendRiver(float[] flow, List<int>[] riverPaths, int index, RiverDef incomingRiver)
		{
			if (riverPaths[index] == null)
			{
				return;
			}
			int bestOutput = riverPaths[index].MaxBy((int ni) => flow[ni]);
			RiverDef riverDef = incomingRiver;
			while (riverDef != null && (float)riverDef.degradeThreshold > flow[bestOutput])
			{
				riverDef = riverDef.degradeChild;
			}
			if (riverDef != null)
			{
				Find.WorldGrid.OverlayRiver(index, bestOutput, riverDef);
				ExtendRiver(flow, riverPaths, bestOutput, riverDef);
			}
			if (incomingRiver.branches == null)
			{
				return;
			}
			foreach (int alternateRiver in riverPaths[index].Where((int ni) => ni != bestOutput))
			{
				RiverDef.Branch branch2 = incomingRiver.branches.Where((RiverDef.Branch branch) => (float)branch.minFlow <= flow[alternateRiver]).MaxByWithFallback((RiverDef.Branch branch) => branch.minFlow);
				if (branch2 != null && Rand.Value < branch2.chance)
				{
					Find.WorldGrid.OverlayRiver(index, alternateRiver, branch2.child);
					ExtendRiver(flow, riverPaths, alternateRiver, branch2.child);
				}
			}
		}

		public static float CalculateEvaporationConstant(float temperature)
		{
			return 0.61121f * Mathf.Exp((18.678f - temperature / 234.5f) * (temperature / (257.14f + temperature))) / (temperature + 273f);
		}

		public static float CalculateRiverSurfaceArea(float flow)
		{
			return Mathf.Pow(flow, 0.5f);
		}

		public static float CalculateEvaporativeArea(float flow)
		{
			return CalculateRiverSurfaceArea(flow) + 0f;
		}

		public static float CalculateTotalEvaporation(float flow, float temperature)
		{
			return CalculateEvaporationConstant(temperature) * CalculateEvaporativeArea(flow) * 250f;
		}
	}
}
