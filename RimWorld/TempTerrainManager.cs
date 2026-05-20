using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class TempTerrainManager : IExposable
{
	private const float MapFractionCheckPerTick = 0.0006f;

	private Map map;

	private FreezeManager freezeManager;

	private int cycleIndex;

	private PriorityQueue<TerrainRemoveTick, int> terrainToRemove = new PriorityQueue<TerrainRemoveTick, int>();

	public TempTerrainManager(Map map)
	{
		this.map = map;
		if (ModsConfig.OdysseyActive)
		{
			freezeManager = new FreezeManager(map);
		}
	}

	public void Tick()
	{
		int num = Mathf.CeilToInt((float)map.Area * 0.0006f);
		int area = map.Area;
		using (new ProfilerBlock("Temporary Terrain Steady Effects"))
		{
			for (int i = 0; i < num; i++)
			{
				if (cycleIndex >= area)
				{
					cycleIndex = 0;
				}
				IntVec3 c = map.cellsInRandomOrder.Get(cycleIndex);
				DoCellSteadyEffects(c);
				cycleIndex++;
			}
		}
		using (new ProfilerBlock("Temporary Terrain Removal"))
		{
			while (terrainToRemove.Count > 0 && terrainToRemove.Peek().tick <= Find.TickManager.TicksGame)
			{
				IntVec3 cell = terrainToRemove.Dequeue().cell;
				map.terrainGrid.RemoveTempTerrain(cell);
			}
		}
	}

	private void DoCellSteadyEffects(IntVec3 c)
	{
		if (ModsConfig.OdysseyActive)
		{
			freezeManager.DoCellSteadyEffects(c);
		}
	}

	public void QueueRemoveTerrain(IntVec3 c, int tick)
	{
		terrainToRemove.Enqueue(new TerrainRemoveTick
		{
			cell = c,
			tick = tick
		}, tick);
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref cycleIndex, "cycleIndex", 0);
		Scribe_Deep.Look(ref freezeManager, "freezeManager");
		List<IntVec3> list = new List<IntVec3>();
		List<int> list2 = new List<int>();
		while (terrainToRemove.Count > 0)
		{
			TerrainRemoveTick terrainRemoveTick = terrainToRemove.Dequeue();
			list.Add(terrainRemoveTick.cell);
			list2.Add(terrainRemoveTick.tick);
		}
		Scribe_Collections.Look(ref list, "terrainToRemoveCells", LookMode.Value);
		Scribe_Collections.Look(ref list2, "terrainToRemoveTicks", LookMode.Value);
		for (int i = 0; i < list.Count; i++)
		{
			terrainToRemove.Enqueue(new TerrainRemoveTick
			{
				cell = list[i],
				tick = list2[i]
			}, list2[i]);
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && freezeManager == null)
		{
			freezeManager = new FreezeManager(map);
		}
	}
}
