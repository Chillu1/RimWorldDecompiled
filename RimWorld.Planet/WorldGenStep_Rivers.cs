using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Rivers : WorldGenStep
{
	private const float HillinessSmallHillsElevation = 15f;

	private const float HillinessLargeHillsElevation = 250f;

	private const float HillinessMountainousElevation = 500f;

	private const float HillinessImpassableElevation = 1000f;

	private const float NonRiverEvaporation = 0f;

	private const float EvaporationMultiple = 250f;

	private static readonly SimpleCurve ElevationChangeCost = new SimpleCurve
	{
		new CurvePoint(-1000f, 50f),
		new CurvePoint(-100f, 100f),
		new CurvePoint(0f, 400f),
		new CurvePoint(0f, 5000f),
		new CurvePoint(100f, 50000f),
		new CurvePoint(1000f, 50000f)
	};

	public override int SeedPart => 605014749;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		GenerateRivers(layer);
	}

	private void GenerateRivers(PlanetLayer layer)
	{
		Find.WorldPathGrid.RecalculateLayerPerceivedPathCosts(layer);
		List<PlanetTile> coastalWaterTiles = GetCoastalWaterTiles(layer);
		if (!coastalWaterTiles.Any())
		{
			return;
		}
		List<PlanetTile> neighbors = new List<PlanetTile>();
		List<PlanetTile>[] array = layer.Pather.FloodPathsWithCostForTree(coastalWaterTiles, delegate(PlanetTile st, PlanetTile ed)
		{
			Tile tile = Find.WorldGrid[ed];
			Tile tile2 = Find.WorldGrid[st];
			Find.WorldGrid.GetTileNeighbors(ed, neighbors);
			PlanetTile planetTile = neighbors[0];
			for (int i = 0; i < neighbors.Count; i++)
			{
				if (GetImpliedElevation(Find.WorldGrid[neighbors[i]]) < GetImpliedElevation(Find.WorldGrid[planetTile]))
				{
					planetTile = neighbors[i];
				}
			}
			float num2 = 1f;
			if (planetTile != st)
			{
				num2 = 2f;
			}
			return Mathf.RoundToInt(num2 * ElevationChangeCost.Evaluate(GetImpliedElevation(tile2) - GetImpliedElevation(tile)));
		}, (PlanetTile tid) => Find.WorldGrid[tid].WaterCovered);
		float[] flow = new float[array.Length];
		for (int num = 0; num < coastalWaterTiles.Count; num++)
		{
			AccumulateFlow(flow, array, coastalWaterTiles[num]);
			CreateRivers(flow, array, coastalWaterTiles[num]);
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

	private List<PlanetTile> GetCoastalWaterTiles(PlanetLayer layer)
	{
		List<PlanetTile> list = new List<PlanetTile>();
		List<PlanetTile> list2 = new List<PlanetTile>();
		for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
		{
			PlanetTile planetTile = new PlanetTile(i, layer);
			if (Find.WorldGrid[i].PrimaryBiome != BiomeDefOf.Ocean)
			{
				continue;
			}
			Find.WorldGrid.GetTileNeighbors(planetTile, list2);
			bool flag = false;
			for (int j = 0; j < list2.Count; j++)
			{
				if (Find.WorldGrid[list2[j]].PrimaryBiome != BiomeDefOf.Ocean)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Add(planetTile);
			}
		}
		return list;
	}

	private void AccumulateFlow(float[] flow, List<PlanetTile>[] riverPaths, PlanetTile planetTile)
	{
		Tile tile = Find.WorldGrid[planetTile];
		int tileId = planetTile.tileId;
		flow[tileId] += tile.rainfall;
		if (riverPaths[tileId] != null)
		{
			for (int i = 0; i < riverPaths[tileId].Count; i++)
			{
				AccumulateFlow(flow, riverPaths, riverPaths[tileId][i]);
				flow[tileId] += flow[riverPaths[tileId][i].tileId];
			}
		}
		flow[tileId] = Mathf.Max(0f, flow[tileId] - CalculateTotalEvaporation(flow[tileId], tile.temperature));
	}

	private void CreateRivers(float[] flow, List<PlanetTile>[] riverPaths, PlanetTile index)
	{
		List<PlanetTile> list = new List<PlanetTile>();
		Find.WorldGrid.GetTileNeighbors(index, list);
		for (int i = 0; i < list.Count; i++)
		{
			float targetFlow = flow[list[i].tileId];
			RiverDef riverDef = DefDatabase<RiverDef>.AllDefs.Where((RiverDef rd) => rd.spawnFlowThreshold > 0 && (float)rd.spawnFlowThreshold <= targetFlow).MaxByWithFallback((RiverDef rd) => rd.spawnFlowThreshold);
			if (riverDef != null && Rand.Value < riverDef.spawnChance)
			{
				PlanetTile planetTile = list[i];
				Find.WorldGrid.OverlayRiver(index, planetTile, riverDef);
				ExtendRiver(flow, riverPaths, planetTile, riverDef);
			}
		}
	}

	private void ExtendRiver(float[] flow, List<PlanetTile>[] riverPaths, PlanetTile planetTile, RiverDef incomingRiver)
	{
		if (riverPaths[planetTile.tileId] == null)
		{
			return;
		}
		PlanetTile bestOutput = riverPaths[planetTile.tileId].MaxBy((PlanetTile x) => flow[x.tileId]);
		RiverDef riverDef = incomingRiver;
		while (riverDef != null && (float)riverDef.degradeThreshold > flow[bestOutput.tileId])
		{
			riverDef = riverDef.degradeChild;
		}
		if (riverDef != null)
		{
			Find.WorldGrid.OverlayRiver(planetTile, bestOutput, riverDef);
			ExtendRiver(flow, riverPaths, bestOutput, riverDef);
		}
		if (incomingRiver.branches == null)
		{
			return;
		}
		foreach (PlanetTile alternateRiver in riverPaths[planetTile.tileId].Where((PlanetTile ni) => ni != bestOutput))
		{
			RiverDef.Branch branch = incomingRiver.branches.Where((RiverDef.Branch branch2) => (float)branch2.minFlow <= flow[alternateRiver.tileId]).MaxByWithFallback((RiverDef.Branch branch2) => branch2.minFlow);
			if (branch != null && Rand.Value < branch.chance)
			{
				Find.WorldGrid.OverlayRiver(planetTile, alternateRiver, branch.child);
				ExtendRiver(flow, riverPaths, alternateRiver, branch.child);
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
