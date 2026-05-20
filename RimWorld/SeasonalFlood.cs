using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class SeasonalFlood : Flood
{
	private const int RecedeIntervalTicks = 1;

	private static readonly IntRange FloodedTicksRange = new IntRange(240000, 360000);

	private int remainFloodedTicks;

	protected override int MaxFloodDurationTicks => 240000;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref remainFloodedTicks, "floodedTicks", 0);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			remainFloodedTicks = FloodedTicksRange.RandomInRange;
			CellFinder.TryFindRandomCell(map, (IntVec3 c) => c.GetTerrain(map).IsRiver && !c.Fogged(map), out var result);
			Find.LetterStack.ReceiveLetter("LetterLabelSeasonalFlooding".Translate(), "LetterSeasonalFlooding".Translate(), LetterDefOf.ThreatSmall, new LookTargets(result, map));
		}
	}

	protected override void Tick()
	{
		if (base.Map.mapTemperature.OutdoorTemp < -7f)
		{
			Messages.Message("MessageSeasonalFloodingFrozen".Translate(), MessageTypeDefOf.NeutralEvent);
			Destroy();
			return;
		}
		base.Tick();
		if (!base.Destroyed && remainFloodedTicks > 0 && Find.TickManager.TicksGame > spawnedTick + base.FloodingTicks + remainFloodedTicks)
		{
			Messages.Message("MessageFloodingSubsiding".Translate(), MessageTypeDefOf.NeutralEvent);
			Destroy();
		}
	}

	protected override IEnumerable<(IntVec3, int)> GetInitialCells(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			yield break;
		}
		List<IntVec3> list = new List<IntVec3>();
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (allCell.GetTerrain(map).IsRiver)
			{
				list.Add(allCell);
			}
		}
		foreach (IntVec3 item in list.ToList())
		{
			bool flag = false;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			foreach (IntVec3 intVec in cardinalDirections)
			{
				IntVec3 c = item + intVec;
				if (c.InBounds(base.Map) && !c.GetTerrain(map).IsRiver)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
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
		int num = estimatedFloodedTiles - floodedTileCount;
		base.Map.terrainGrid.SetTempTerrain(cell, TerrainDefOf.ShallowFloodwater);
		base.Map.tempTerrain.QueueRemoveTerrain(cell, spawnedTick + base.FloodingTicks + remainFloodedTicks + num);
	}
}
