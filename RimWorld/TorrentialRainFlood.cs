using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class TorrentialRainFlood : Flood
{
	private const int RecedeIntervalTicks = 10;

	private Dictionary<IntVec3, int> floodedCells = new Dictionary<IntVec3, int>();

	private const int EndDelayTicks = 2000;

	private List<IntVec3> tmpKeys;

	private List<int> tmpValues;

	protected override int StartDelayTicks => 4000;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref floodedCells, "floodedCells", LookMode.Value, LookMode.Value, ref tmpKeys, ref tmpValues);
	}

	protected override IEnumerable<(IntVec3, int)> GetInitialCells(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			yield break;
		}
		List<IntVec3> list = new List<IntVec3>();
		foreach (WaterBody body in map.waterBodyTracker.Bodies)
		{
			if (body.waterBodyType == WaterBodyType.Saltwater)
			{
				continue;
			}
			bool flag = false;
			foreach (IntVec3 cell in body.cells)
			{
				if (!cell.Roofed(map))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (IntVec3 cell2 in body.cells)
			{
				if (cell2.GetTerrain(map).IsWater && !floodedCells.ContainsKey(cell2))
				{
					list.Add(cell2);
				}
			}
		}
		if (list.Count == 0)
		{
			yield break;
		}
		foreach (IntVec3 item in list.ToList())
		{
			bool flag2 = false;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			foreach (IntVec3 intVec in cardinalDirections)
			{
				IntVec3 c = item + intVec;
				if (c.InBounds(base.Map) && !c.GetTerrain(map).IsWater)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				list.Remove(item);
			}
		}
		list.Shuffle();
		foreach (IntVec3 item2 in list)
		{
			yield return (item2, Flood.FloodWidthRange.RandomInRange);
		}
	}

	protected override void SpreadFlood(IntVec3 cell, TerrainDef sourceTerrain)
	{
		int value = (estimatedFloodedTiles - floodedTileCount) * 10;
		base.Map.terrainGrid.SetTempTerrain(cell, sourceTerrain.floodTerrain);
		floodedCells[cell] = value;
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		if (floodedCells.Any())
		{
			int num = floodedCells.Values.Min();
			foreach (var (c, num3) in floodedCells)
			{
				base.Map.tempTerrain.QueueRemoveTerrain(c, Find.TickManager.TicksGame + 2000 + num3 - num);
			}
		}
		base.Destroy(mode);
	}
}
