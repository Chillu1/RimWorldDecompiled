using System.Collections.Generic;
using System.Text;
using RimWorld;

namespace Verse.AI
{
	public sealed class PathGrid
	{
		private Map map;

		public int[] pathGrid;

		public const int ImpassableCost = 10000;

		public PathGrid(Map map)
		{
			this.map = map;
			ResetPathGrid();
		}

		public void ResetPathGrid()
		{
			pathGrid = new int[map.cellIndices.NumGridCells];
		}

		public bool Walkable(IntVec3 loc)
		{
			if (!loc.InBounds(map))
			{
				return false;
			}
			return pathGrid[map.cellIndices.CellToIndex(loc)] < 10000;
		}

		public bool WalkableFast(IntVec3 loc)
		{
			return pathGrid[map.cellIndices.CellToIndex(loc)] < 10000;
		}

		public bool WalkableFast(int x, int z)
		{
			return pathGrid[map.cellIndices.CellToIndex(x, z)] < 10000;
		}

		public bool WalkableFast(int index)
		{
			return pathGrid[index] < 10000;
		}

		public int PerceivedPathCostAt(IntVec3 loc)
		{
			return pathGrid[map.cellIndices.CellToIndex(loc)];
		}

		public void RecalculatePerceivedPathCostUnderThing(Thing t)
		{
			if (t.def.size == IntVec2.One)
			{
				RecalculatePerceivedPathCostAt(t.Position);
				return;
			}
			CellRect cellRect = t.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					RecalculatePerceivedPathCostAt(c);
				}
			}
		}

		public void RecalculatePerceivedPathCostAt(IntVec3 c)
		{
			if (c.InBounds(map))
			{
				bool flag = WalkableFast(c);
				pathGrid[map.cellIndices.CellToIndex(c)] = CalculatedCostAt(c, perceivedStatic: true, IntVec3.Invalid);
				if (WalkableFast(c) != flag)
				{
					map.reachability.ClearCache();
					map.regionDirtyer.Notify_WalkabilityChanged(c);
				}
			}
		}

		public void RecalculateAllPerceivedPathCosts()
		{
			foreach (IntVec3 allCell in map.AllCells)
			{
				RecalculatePerceivedPathCostAt(allCell);
			}
		}

		public int CalculatedCostAt(IntVec3 c, bool perceivedStatic, IntVec3 prevCell)
		{
			int num = 0;
			bool flag = false;
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
			if (terrainDef == null || terrainDef.passability == Traversability.Impassable)
			{
				return 10000;
			}
			num = terrainDef.pathCost;
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.def.passability == Traversability.Impassable)
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
				if (thing is Building_Door && prevCell.IsValid)
				{
					Building edifice = prevCell.GetEdifice(map);
					if (edifice != null && edifice is Building_Door)
					{
						flag = true;
					}
				}
			}
			int num2 = SnowUtility.MovementTicksAddOn(map.snowGrid.GetCategory(c));
			if (num2 > num)
			{
				num = num2;
			}
			if (flag)
			{
				num += 45;
			}
			if (perceivedStatic)
			{
				for (int j = 0; j < 9; j++)
				{
					IntVec3 b = GenAdj.AdjacentCellsAndInside[j];
					IntVec3 c2 = c + b;
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
						num = ((b.x != 0 || b.z != 0) ? (num + 150) : (num + 1000));
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
}
