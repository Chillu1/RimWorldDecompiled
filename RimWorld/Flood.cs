using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class Flood : Thing
{
	protected static readonly IntRange FloodWidthRange = new IntRange(10, 12);

	private HashSet<IntVec3>[] openCellWeights = new HashSet<IntVec3>[FloodWidthRange.max];

	protected int estimatedFloodedTiles;

	protected int floodedTileCount;

	private bool noPossibleCell;

	private List<IntVec3> tmpOpenCells = new List<IntVec3>();

	private List<int> tmpFloodedWeights = new List<int>();

	private List<IntVec3> tempAdjCells;

	private int ExpandIntervalTicks => MaxFloodDurationTicks / estimatedFloodedTiles;

	protected virtual int StartDelayTicks => 0;

	protected int FloodingTicks => MaxFloodDurationTicks + StartDelayTicks;

	protected virtual int MaxFloodDurationTicks => 120000;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref estimatedFloodedTiles, "estimatedFloodedTiles", 0);
		Scribe_Values.Look(ref floodedTileCount, "floodedTileCount", 0);
		Dictionary<IntVec3, int> dict = new Dictionary<IntVec3, int>();
		int value;
		for (int i = 0; i < openCellWeights.Length; i++)
		{
			HashSet<IntVec3>[] array = openCellWeights;
			value = i;
			if (array[value] == null)
			{
				array[value] = new HashSet<IntVec3>();
			}
			foreach (IntVec3 item2 in openCellWeights[i])
			{
				dict[item2] = i;
			}
		}
		Scribe_Collections.Look(ref dict, "openCellWeights", LookMode.Value, LookMode.Value, ref tmpOpenCells, ref tmpFloodedWeights);
		openCellWeights = new HashSet<IntVec3>[FloodWidthRange.max];
		for (int j = 0; j < openCellWeights.Length; j++)
		{
			openCellWeights[j] = new HashSet<IntVec3>();
		}
		foreach (KeyValuePair<IntVec3, int> item3 in dict)
		{
			item3.Deconstruct(out var key, out value);
			IntVec3 item = key;
			int num = value;
			openCellWeights[num].Add(item);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!ModLister.CheckOdyssey("Flood"))
		{
			return;
		}
		map.events.TerrainChanged += Notify_OnTerrainChanged;
		map.events.BuildingSpawned += Notify_BuildingChanged;
		map.events.BuildingDespawned += Notify_BuildingChanged;
		if (respawningAfterLoad)
		{
			return;
		}
		for (int i = 0; i < openCellWeights.Length; i++)
		{
			openCellWeights[i] = new HashSet<IntVec3>();
		}
		foreach (var (item, num) in GetInitialCells(map))
		{
			openCellWeights[num - 1].Add(item);
		}
		if (!AnyCellOpen())
		{
			Destroy();
			return;
		}
		estimatedFloodedTiles = openCellWeights.Sum((HashSet<IntVec3> c) => c.Count) * FloodWidthRange.max;
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		base.Map.events.TerrainChanged -= Notify_OnTerrainChanged;
		base.Map.events.BuildingSpawned -= Notify_BuildingChanged;
		base.Map.events.BuildingDespawned -= Notify_BuildingChanged;
		base.DeSpawn(mode);
	}

	private void Notify_BuildingChanged(Building building)
	{
		if (CanFloodSpreadInto(building.Position))
		{
			noPossibleCell = false;
		}
	}

	private void Notify_OnTerrainChanged(IntVec3 cell)
	{
		if (CanFloodSpreadInto(cell))
		{
			noPossibleCell = false;
		}
	}

	protected abstract IEnumerable<(IntVec3, int)> GetInitialCells(Map map);

	protected override void Tick()
	{
		if (!noPossibleCell && this.IsHashIntervalTick(ExpandIntervalTicks) && Find.TickManager.TicksGame > spawnedTick + StartDelayTicks)
		{
			SpreadFlood();
		}
	}

	private void SpreadFlood()
	{
		if (tempAdjCells == null)
		{
			tempAdjCells = GenAdj.CardinalDirections.ToList();
		}
		tempAdjCells.Shuffle();
		bool flag = false;
		while (!flag)
		{
			var (intVec, num) = GetNextCell();
			if (!intVec.IsValid)
			{
				noPossibleCell = true;
				break;
			}
			bool flag2 = true;
			TerrainDef terrainDef = base.Map.terrainGrid.TempTerrainAt(intVec);
			if (terrainDef == null || terrainDef.floodTerrain == null)
			{
				terrainDef = base.Map.terrainGrid.BaseTerrainAt(intVec);
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec2 = intVec + GenAdj.CardinalDirections[i];
				if (CanFloodPotentiallySpreadInto(intVec2))
				{
					flag2 = false;
				}
				if (CanFloodSpreadInto(intVec2))
				{
					int num2 = num - 1;
					if (num2 > 0)
					{
						openCellWeights[num2 - 1].Add(intVec2);
					}
					SpreadFlood(intVec2, terrainDef);
					floodedTileCount++;
					flag = true;
					break;
				}
			}
			if (flag2)
			{
				openCellWeights[num - 1].Remove(intVec);
			}
		}
	}

	protected abstract void SpreadFlood(IntVec3 cell, TerrainDef sourceTerrain);

	private bool CanFloodPotentiallySpreadInto(IntVec3 cell)
	{
		if (!cell.InBounds(base.Map))
		{
			return false;
		}
		if (CellOpen(cell))
		{
			return false;
		}
		if (base.Map.terrainGrid.TerrainAt(cell).IsWater)
		{
			return false;
		}
		return true;
	}

	private bool CanFloodSpreadInto(IntVec3 cell)
	{
		if (!CanFloodPotentiallySpreadInto(cell))
		{
			return false;
		}
		if (base.Map.terrainGrid.FoundationAt(cell) != null)
		{
			return false;
		}
		if (cell.GetEdifice(base.Map) != null)
		{
			return false;
		}
		return true;
	}

	private (IntVec3, int) GetNextCell()
	{
		using (ProfilerBlock.Scope("GetNextCell"))
		{
			for (int num = openCellWeights.Length - 1; num >= 0; num--)
			{
				if (openCellWeights[num].Count != 0)
				{
					for (int i = 0; i < 10; i++)
					{
						if (openCellWeights[num].TryRandomElement(out var result) && CellCanSpreadFlood(result))
						{
							return (result, num + 1);
						}
						if (!CellCanPotentiallySpreadFlood(result))
						{
							openCellWeights[num].Remove(result);
						}
					}
					for (int num2 = openCellWeights[num].Count - 1; num2 >= 0; num2--)
					{
						if (openCellWeights[num].TryRandomElement(out var result2) && CellCanSpreadFlood(result2))
						{
							return (result2, num + 1);
						}
						if (!CellCanPotentiallySpreadFlood(result2))
						{
							openCellWeights[num].Remove(result2);
						}
					}
				}
			}
			return (IntVec3.Invalid, -1);
		}
	}

	private bool CellCanSpreadFlood(IntVec3 cell)
	{
		for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
		{
			IntVec3 intVec = GenAdj.CardinalDirections[i];
			if (CanFloodSpreadInto(cell + intVec))
			{
				return true;
			}
		}
		return false;
	}

	private bool CellCanPotentiallySpreadFlood(IntVec3 cell)
	{
		for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
		{
			IntVec3 intVec = GenAdj.CardinalDirections[i];
			if (CanFloodPotentiallySpreadInto(cell + intVec))
			{
				return true;
			}
		}
		return false;
	}

	private bool CellOpen(IntVec3 cell)
	{
		for (int i = 0; i < openCellWeights.Length; i++)
		{
			if (openCellWeights[i].Contains(cell))
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyCellOpen()
	{
		for (int i = 0; i < openCellWeights.Length; i++)
		{
			if (openCellWeights[i].Count > 0)
			{
				return true;
			}
		}
		return false;
	}
}
