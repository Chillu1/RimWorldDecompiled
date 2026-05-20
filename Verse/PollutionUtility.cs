using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Noise;

namespace Verse;

public static class PollutionUtility
{
	internal class PollutionCellComparer : IComparer<IntVec3>
	{
		private const float NoisyEdgeFactor = 0.25f;

		private const float PerlinNoiseFactor = 2f;

		private readonly IntVec3 root;

		private readonly Map map;

		private readonly ModuleBase perlin;

		public PollutionCellComparer(IntVec3 root, Map map, float frequency = 0.015f)
		{
			this.root = root;
			this.map = map;
			perlin = new Perlin(frequency, 2.0, 0.5, 6, map.uniqueID, QualityMode.Medium);
			perlin = new ScaleBias(0.5, 0.5, perlin);
		}

		private float PolluteScore(IntVec3 c)
		{
			float num = 1f;
			num *= 1f / c.DistanceTo(root);
			num *= 1f + (float)perlin.GetValue(c.x, c.y, c.z) * 2f;
			if (MapGenerator.mapBeingGenerated == map)
			{
				num *= c.DistanceTo(MapGenerator.PlayerStartSpot) / map.Size.LengthHorizontal;
			}
			return num * (1f + (float)AdjacentPollutedCount(c) / 8f * 0.25f);
		}

		private int AdjacentPollutedCount(IntVec3 c)
		{
			int num = 0;
			for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
			{
				IntVec3 intVec = GenAdj.AdjacentCells[i];
				IntVec3 c2 = c + intVec;
				if (c2.InBounds(map) && c2.IsPolluted(map))
				{
					num++;
				}
			}
			return num;
		}

		public int Compare(IntVec3 a, IntVec3 b)
		{
			float num = PolluteScore(a);
			float num2 = PolluteScore(b);
			if (num < num2)
			{
				return 1;
			}
			if (num > num2)
			{
				return -1;
			}
			return 0;
		}
	}

	private const float PollutionPerlinFrequency = 0.015f;

	private static FastPriorityQueue<IntVec3> tmpPollutableCells;

	private static readonly List<IntVec3> tmpPollutedCells = new List<IntVec3>();

	private static readonly List<IntVec3> tmpMapCells = new List<IntVec3>();

	public static void PawnPollutionTickInterval(Pawn pawn, int delta)
	{
		if (pawn.Spawned && pawn.IsHashIntervalTick(60, delta) && pawn.Position.IsPolluted(pawn.Map) && StimulatedByPollution(pawn) && !pawn.health.hediffSet.HasHediff(HediffDefOf.PollutionStimulus))
		{
			pawn.health.AddHediff(HediffDefOf.PollutionStimulus);
		}
	}

	public static void Notify_TunnelHiveSpawnedInsect(Pawn pawn)
	{
		if (pawn.Spawned && pawn.RaceProps.Insect && pawn.Position.IsPolluted(pawn.Map))
		{
			pawn.health.AddHediff(HediffDefOf.PollutionStimulus);
			HealthUtility.AdjustSeverity(pawn, HediffDefOf.PollutionStimulus, HediffDefOf.PollutionStimulus.maxSeverity);
		}
	}

	private static bool StimulatedByPollution(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (pawn.RaceProps.Insect)
		{
			return true;
		}
		if (pawn.genes?.GetFirstGeneOfType<Gene_PollutionRush>() != null)
		{
			return true;
		}
		return false;
	}

	public static bool SettableEntirelyPolluted(IPlantToGrowSettable s)
	{
		foreach (IntVec3 cell in s.Cells)
		{
			if (!cell.IsPolluted(s.Map))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanPlantAt(ThingDef plantDef, IPlantToGrowSettable settable)
	{
		if (plantDef.plant.RequiresNoPollution)
		{
			foreach (IntVec3 cell in settable.Cells)
			{
				if (!cell.IsPolluted(settable.Map))
				{
					return true;
				}
			}
			return false;
		}
		if (plantDef.plant.RequiresPollution)
		{
			foreach (IntVec3 cell2 in settable.Cells)
			{
				if (cell2.IsPolluted(settable.Map))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool IsExecutingPollutionIgnoredQuest()
	{
		foreach (Quest item in Find.QuestManager.QuestsListForReading)
		{
			if (item.State == QuestState.Ongoing && (item.root == QuestScriptDefOf.EndGame_ArchonexusVictory_FirstCycle || item.root == QuestScriptDefOf.EndGame_ArchonexusVictory_SecondCycle))
			{
				return true;
			}
		}
		return false;
	}

	public static void GrowPollutionAt(IntVec3 root, Map map, int cellCountToPollute = 4, Action<IntVec3> onPolluteAction = null, bool silent = false, Func<IntVec3, bool> adjacentPolluteValidator = null)
	{
		if (cellCountToPollute < 0)
		{
			Log.Error("Non-positive max cells value passed in PolluteClosestMapTerrain");
		}
		else
		{
			if (map.pollutionGrid.TotalPollution >= map.cellIndices.NumGridCells)
			{
				return;
			}
			if (root.CanPollute(map))
			{
				root.Pollute(map);
				cellCountToPollute--;
				onPolluteAction?.Invoke(root);
			}
			if (cellCountToPollute <= 0)
			{
				return;
			}
			tmpPollutableCells = new FastPriorityQueue<IntVec3>(new PollutionCellComparer(root, map));
			map.floodFiller.FloodFill(root, (IntVec3 x) => x.IsPolluted(map), delegate(IntVec3 x)
			{
				tmpPollutedCells.Add(x);
			});
			if (tmpPollutedCells.Count == 0)
			{
				return;
			}
			tmpPollutableCells.Clear();
			for (int num = 0; num < tmpPollutedCells.Count; num++)
			{
				foreach (IntVec3 item in GetAdjacentPollutableCells(tmpPollutedCells[num], map))
				{
					if (!tmpPollutableCells.Contains(item))
					{
						tmpPollutableCells.Push(item);
					}
				}
			}
			tmpPollutedCells.Clear();
			while (cellCountToPollute > 0 && tmpPollutableCells.Count > 0)
			{
				IntVec3 intVec = tmpPollutableCells.Pop();
				map.pollutionGrid.SetPolluted(intVec, isPolluted: true, silent);
				onPolluteAction?.Invoke(intVec);
				foreach (IntVec3 item2 in GetAdjacentPollutableCells(intVec, map))
				{
					if (!tmpPollutableCells.Contains(item2))
					{
						tmpPollutableCells.Push(item2);
					}
				}
				cellCountToPollute--;
			}
		}
		IEnumerable<IntVec3> GetAdjacentPollutableCells(IntVec3 c, Map m)
		{
			for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
			{
				IntVec3 intVec2 = GenAdj.CardinalDirections[i];
				IntVec3 intVec3 = c + intVec2;
				if (intVec3.InBounds(map) && intVec3.CanPollute(map) && (adjacentPolluteValidator == null || adjacentPolluteValidator(intVec3)))
				{
					yield return intVec3;
				}
			}
		}
	}

	public static PollutionLevel PollutionLevel(this Tile tile)
	{
		if (tile.pollution < 0.25f)
		{
			return Verse.PollutionLevel.None;
		}
		if (tile.pollution < 0.5f)
		{
			return Verse.PollutionLevel.Light;
		}
		if (tile.pollution < 0.75f)
		{
			return Verse.PollutionLevel.Moderate;
		}
		return Verse.PollutionLevel.Extreme;
	}

	public static void PolluteMapToPercent(Map map, float mapPollutionPercent, int globSize = 1000)
	{
		mapPollutionPercent = Mathf.Clamp01(mapPollutionPercent);
		float totalPollutionPercent = map.pollutionGrid.TotalPollutionPercent;
		if (Mathf.Approximately(mapPollutionPercent, totalPollutionPercent))
		{
			return;
		}
		List<IntVec3> allPollutableCells = map.pollutionGrid.AllPollutableCells;
		if (mapPollutionPercent < totalPollutionPercent)
		{
			tmpMapCells.Clear();
			tmpMapCells.AddRange(allPollutableCells.InRandomOrder());
			float num = totalPollutionPercent - mapPollutionPercent;
			float num2 = Mathf.Max(0f, (float)tmpMapCells.Count * num);
			for (int i = 0; (float)i < num2; i++)
			{
				tmpMapCells[i].Unpollute(map);
			}
			tmpMapCells.Clear();
		}
		else
		{
			int num3 = Mathf.FloorToInt((float)allPollutableCells.Count * mapPollutionPercent);
			int num4 = Mathf.CeilToInt((float)num3 / (float)globSize);
			for (int j = 0; j < num4; j++)
			{
				int num5 = Mathf.Min(globSize, num3);
				GrowPollutionAt(allPollutableCells.RandomElementByWeight(GlobCellSelectionWeight), map, num5);
				num3 -= num5;
			}
		}
		float GlobCellSelectionWeight(IntVec3 c)
		{
			return 1f * (c.DistanceTo(map.Center) / map.Size.LengthHorizontal) * ((float)c.DistanceToEdge(map) / map.Size.LengthHorizontal);
		}
	}
}
