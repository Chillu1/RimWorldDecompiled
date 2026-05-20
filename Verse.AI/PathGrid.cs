using System;
using System.Collections.Generic;
using System.Text;
using LudeonTK;
using RimWorld;
using Unity.Collections;

namespace Verse.AI;

public class PathGrid : IDisposable
{
	public readonly Map map;

	public readonly PathGridDef def;

	private NativeArray<int> pathGrid;

	private NativeBitArray dirty;

	public const int ImpassableCost = 10000;

	public NativeArray<int> Grid_Unsafe => pathGrid;

	public PathGrid(Map map, PathGridDef def)
	{
		this.map = map;
		this.def = def;
		pathGrid = new NativeArray<int>(map.cellIndices.NumGridCells, Allocator.Persistent);
	}

	public bool Walkable(IntVec3 loc)
	{
		if (!loc.InBounds(map))
		{
			return false;
		}
		return Cost(loc) < 10000;
	}

	public bool WalkableFast(IntVec3 loc)
	{
		return Cost(loc) < 10000;
	}

	public bool WalkableFast(int x, int z)
	{
		return pathGrid[map.cellIndices.CellToIndex(x, z)] < 10000;
	}

	public bool WalkableFast(int index)
	{
		return pathGrid[index] < 10000;
	}

	public int Cost(IntVec3 loc)
	{
		return pathGrid[map.cellIndices.CellToIndex(loc)];
	}

	private void RecalculatePerceivedPathCostAt(IntVec3 c)
	{
		bool haveNotified = false;
		RecalculatePerceivedPathCostAt(c, ref haveNotified);
	}

	public void RecalculatePerceivedPathCostAt(IntVec3 c, ref bool haveNotified)
	{
		if (!c.InBounds(map))
		{
			return;
		}
		if (dirty.IsCreated)
		{
			dirty.Set(map.cellIndices.CellToIndex(c), value: true);
		}
		else if (!haveNotified)
		{
			if (RecalcuateCellDirect(c))
			{
				NotifyCellDirtied(c);
				haveNotified = true;
			}
		}
		else
		{
			pathGrid[map.cellIndices.CellToIndex(c)] = CalculatedCostAt(c, perceivedStatic: true, IntVec3.Invalid);
		}
	}

	private bool RecalcuateCellDirect(IntVec3 c)
	{
		if (!c.InBounds(map))
		{
			return false;
		}
		int index = map.cellIndices[c];
		bool num = WalkableFast(index);
		pathGrid[index] = CalculatedCostAt(c, perceivedStatic: true, IntVec3.Invalid);
		return num != WalkableFast(index);
	}

	private void NotifyCellDirtied(IntVec3 cell)
	{
		map.reachability.ClearCache();
		map.regionDirtyer.Notify_WalkabilityChanged(cell, WalkableFast(cell));
	}

	public void RecalculateAllPerceivedPathCosts()
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			RecalculatePerceivedPathCostAt(allCell);
		}
		if (dirty.IsCreated)
		{
			dirty.Clear();
		}
	}

	public virtual int CalculatedCostAt(IntVec3 c, bool perceivedStatic, IntVec3 prevCell, int? baseCostOverride = null)
	{
		int num = 0;
		bool flag = false;
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
		if (terrainDef == null || (terrainDef.passability == Traversability.Impassable && (!def.flying || !terrainDef.forcePassableByFlyingPawns)))
		{
			return 10000;
		}
		if (baseCostOverride.HasValue)
		{
			num = baseCostOverride.Value;
		}
		else if (!def.flying)
		{
			num = terrainDef.pathCost;
		}
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (thing.def.passability == Traversability.Impassable && (!def.flying || !thing.def.forcePassableByFlyingPawns))
			{
				return 10000;
			}
			if (!def.fencePassable && thing.def.building != null && thing.def.building.isFence)
			{
				return 10000;
			}
			if (!IsPathCostIgnoreRepeater(thing.def) || !prevCell.IsValid || !ContainsPathCostIgnoreRepeater(prevCell))
			{
				int pathCost = thing.def.pathCost;
				if (pathCost > num)
				{
					num = pathCost;
				}
			}
			if (thing is Building_Door building_Door && prevCell.IsValid && prevCell.GetEdifice(map) is Building_Door building_Door2 && !building_Door.FreePassage && !building_Door2.FreePassage)
			{
				flag = true;
			}
		}
		int num2 = WeatherBuildupUtility.MovementTicksAddOn(map.snowGrid.GetCategory(c));
		if (num2 > num)
		{
			num = num2;
		}
		if (ModsConfig.OdysseyActive)
		{
			int num3 = WeatherBuildupUtility.MovementTicksAddOn(map.sandGrid.GetCategory(c));
			if (num3 > num)
			{
				num = num3;
			}
		}
		if (flag)
		{
			num += 45;
		}
		if (perceivedStatic)
		{
			for (int j = 0; j < 9; j++)
			{
				IntVec3 intVec = GenAdj.AdjacentCellsAndInside[j];
				IntVec3 c2 = c + intVec;
				if (!c2.InBounds(map))
				{
					continue;
				}
				Fire fire = null;
				list = map.thingGrid.ThingsListAtFast(c2);
				for (int k = 0; k < list.Count; k++)
				{
					fire = list[k] as Fire;
					if (fire != null)
					{
						break;
					}
				}
				if (fire != null && fire.parent == null)
				{
					num = ((intVec.x != 0 || intVec.z != 0) ? (num + 150) : (num + 1000));
				}
			}
		}
		return num;
	}

	private bool ContainsPathCostIgnoreRepeater(IntVec3 c)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (IsPathCostIgnoreRepeater(list[i].def))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsPathCostIgnoreRepeater(ThingDef def)
	{
		if (def.pathCost >= 25)
		{
			return def.pathCostIgnoreRepeat;
		}
		return false;
	}

	public void DisableIncrementalDirtying()
	{
		if (!dirty.IsCreated)
		{
			dirty = new NativeBitArray(map.cellIndices.NumGridCells, Allocator.Persistent);
		}
	}

	public void ReEnableIncrementalDirtying()
	{
		if (!dirty.IsCreated)
		{
			return;
		}
		for (int i = 0; i < dirty.Length; i++)
		{
			if (dirty.IsSet(i))
			{
				IntVec3 intVec = map.cellIndices[i];
				RecalcuateCellDirect(intVec);
				NotifyCellDirtied(intVec);
			}
		}
		dirty.Dispose();
	}

	public void Dispose()
	{
		NativeArrayUtility.EnsureDisposed(ref pathGrid);
		dirty.EnsureDisposed();
	}

	[DebugOutput]
	public static void ThingPathCostsIgnoreRepeaters()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("===============PATH COST IGNORE REPEATERS==============");
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (IsPathCostIgnoreRepeater(allDef) && allDef.passability != Traversability.Impassable)
			{
				stringBuilder.AppendLine(allDef.defName + " " + allDef.pathCost);
			}
		}
		stringBuilder.AppendLine("===============NON-PATH COST IGNORE REPEATERS that are buildings with >0 pathCost ==============");
		foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
		{
			if (!IsPathCostIgnoreRepeater(allDef2) && allDef2.passability != Traversability.Impassable && allDef2.category == ThingCategory.Building && allDef2.pathCost > 0)
			{
				stringBuilder.AppendLine(allDef2.defName + " " + allDef2.pathCost);
			}
		}
		Log.Message(stringBuilder.ToString());
	}
}
