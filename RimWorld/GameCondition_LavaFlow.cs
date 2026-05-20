using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GameCondition_LavaFlow : GameCondition
{
	private HashSet<IntVec3> openCells = new HashSet<IntVec3>();

	private int maxFlow;

	private const int FlowRateTicks = 5;

	private const int CoolDurationTicks = 60000;

	private static readonly IntRange StartPositionCountRange = new IntRange(3, 6);

	private static readonly IntRange MaxFlowRange = new IntRange(20000, 32000);

	private List<IntVec3> tempAdjCells;

	private int FlowDurationTicks => maxFlow * 5;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref openCells, "openCells", LookMode.Value);
		Scribe_Values.Look(ref maxFlow, "maxFlow", 0);
	}

	public override void Init()
	{
		base.Init();
		maxFlow = MaxFlowRange.RandomInRange;
		foreach (IntVec3 initialCell in GetInitialCells(base.SingleMap))
		{
			base.SingleMap.terrainGrid.SetTempTerrain(initialCell, TerrainDefOf.LavaShallow);
			base.SingleMap.tempTerrain.QueueRemoveTerrain(initialCell, Find.TickManager.TicksGame + FlowDurationTicks + 60000);
			openCells.Add(initialCell);
		}
		if (openCells.Count == 0)
		{
			End();
		}
		else
		{
			Find.LetterStack.ReceiveLetter("LetterLabelLavaFlow".Translate(), "LetterLavaFlow".Translate(), LetterDefOf.ThreatSmall, new LookTargets(openCells.First(), base.SingleMap));
		}
	}

	private IEnumerable<IntVec3> GetInitialCells(Map map)
	{
		Rot4 edge = Rot4.Random;
		int count = StartPositionCountRange.RandomInRange;
		for (int i = 0; i < count; i++)
		{
			if (CellFinder.TryFindRandomEdgeCellWith(delegate(IntVec3 c)
			{
				if (c.GetTerrain(map) != TerrainDefOf.VolcanicRock && c.GetTerrain(map) != TerrainDefOf.CooledLava)
				{
					return false;
				}
				if (c.Fogged(map))
				{
					return false;
				}
				return c.GetEdifice(map) == null;
			}, map, edge, 0f, out var result))
			{
				yield return result;
			}
		}
	}

	public override void GameConditionTick()
	{
		if (base.SingleMap.IsHashIntervalTick(5))
		{
			SpreadLava();
			if (Find.TickManager.TicksGame > startTick + FlowDurationTicks || openCells.Count == 0)
			{
				End();
			}
		}
	}

	private void SpreadLava()
	{
		if (!openCells.TryRandomElementByWeight((IntVec3 c) => Mathf.Max(AdjacentLavaCells(c), 1) * 4, out var result))
		{
			result = openCells.RandomElement();
		}
		if (tempAdjCells == null)
		{
			tempAdjCells = GenAdj.CardinalDirections.ToList();
		}
		tempAdjCells.Shuffle();
		bool flag = true;
		foreach (IntVec3 tempAdjCell in tempAdjCells)
		{
			IntVec3 intVec = result + tempAdjCell;
			if (CanLavaPotentiallySpreadInto(intVec))
			{
				flag = false;
			}
			if (CanLavaSpreadInto(intVec))
			{
				base.SingleMap.terrainGrid.SetTempTerrain(intVec, TerrainDefOf.LavaShallow);
				base.SingleMap.tempTerrain.QueueRemoveTerrain(intVec, Find.TickManager.TicksGame + FlowDurationTicks + 60000);
				openCells.Add(intVec);
				break;
			}
		}
		if (flag)
		{
			openCells.Remove(result);
		}
	}

	private bool CanLavaPotentiallySpreadInto(IntVec3 c)
	{
		if (!c.InBounds(base.SingleMap))
		{
			return false;
		}
		if (openCells.Contains(c))
		{
			return false;
		}
		if (base.SingleMap.terrainGrid.FoundationAt(c) != null)
		{
			return false;
		}
		if ((base.SingleMap.terrainGrid.UnderTerrainAt(c) ?? base.SingleMap.terrainGrid.TopTerrainAt(c)) != TerrainDefOf.VolcanicRock)
		{
			return false;
		}
		if (base.SingleMap.terrainGrid.TerrainAt(c) == TerrainDefOf.LavaShallow)
		{
			return false;
		}
		return true;
	}

	private bool CanLavaSpreadInto(IntVec3 c)
	{
		if (!CanLavaPotentiallySpreadInto(c))
		{
			return false;
		}
		Building edifice = c.GetEdifice(base.SingleMap);
		if (edifice != null && !edifice.IsClearableFreeBuilding)
		{
			return false;
		}
		return true;
	}

	private int AdjacentLavaCells(IntVec3 c)
	{
		int num = 0;
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		foreach (IntVec3 intVec in cardinalDirections)
		{
			IntVec3 c2 = c + intVec;
			if (c2.InBounds(base.SingleMap) && c2.GetTerrain(base.SingleMap) == TerrainDefOf.LavaShallow)
			{
				num++;
			}
		}
		return num;
	}
}
