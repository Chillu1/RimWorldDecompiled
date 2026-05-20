using System.Collections.Generic;

namespace Verse;

public static class RoofCollapseCellsFinder
{
	private static List<IntVec3> roofsCollapsingBecauseTooFar = new List<IntVec3>();

	private static HashSet<IntVec3> visitedCells = new HashSet<IntVec3>();

	public static void Notify_RoofHolderDespawned(Thing t, Map map)
	{
		if (Current.ProgramState == ProgramState.Playing)
		{
			ProcessRoofHolderDespawned(t.OccupiedRect(), t.Position, map);
		}
	}

	public static void ProcessRoofHolderDespawned(CellRect rect, IntVec3 position, Map map, bool removalMode = false, bool canRemoveThickRoof = false)
	{
		CheckCollapseFlyingRoofs(rect, map, removalMode, canRemoveThickRoof);
		RoofGrid roofGrid = map.roofGrid;
		roofsCollapsingBecauseTooFar.Clear();
		for (int i = 0; i < RoofCollapseUtility.RoofSupportRadialCellsCount; i++)
		{
			IntVec3 intVec = position + GenRadial.RadialPattern[i];
			if (intVec.InBounds(map) && roofGrid.Roofed(intVec.x, intVec.z) && roofGrid.RoofAt(intVec).canCollapse && !map.roofCollapseBuffer.IsMarkedToCollapse(intVec) && !RoofCollapseUtility.WithinRangeOfRoofHolder(intVec, map))
			{
				if (removalMode && (canRemoveThickRoof || intVec.GetRoof(map).VanishOnCollapse))
				{
					map.roofGrid.SetRoof(intVec, null);
				}
				else
				{
					map.roofCollapseBuffer.MarkToCollapse(intVec);
				}
				roofsCollapsingBecauseTooFar.Add(intVec);
			}
		}
		CheckCollapseFlyingRoofs(roofsCollapsingBecauseTooFar, map, removalMode, canRemoveThickRoof);
		roofsCollapsingBecauseTooFar.Clear();
	}

	public static void CheckAndRemoveCollpsingRoofs(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (allCell.Roofed(map))
			{
				Building edifice = allCell.GetEdifice(map);
				if (edifice != null && edifice.def.holdsRoof)
				{
					ProcessRoofHolderDespawned(new CellRect(allCell.x, allCell.z, 1, 1), allCell, map, removalMode: true, canRemoveThickRoof: true);
				}
			}
		}
	}

	public static void RemoveBulkCollapsingRoofs(List<IntVec3> nearCells, Map map)
	{
		for (int i = 0; i < nearCells.Count; i++)
		{
			ProcessRoofHolderDespawned(new CellRect(nearCells[i].x, nearCells[i].z, 1, 1), nearCells[i], map, removalMode: true, canRemoveThickRoof: true);
		}
	}

	public static void CheckCollapseFlyingRoofs(List<IntVec3> nearCells, Map map, bool removalMode = false, bool canRemoveThickRoof = false)
	{
		visitedCells.Clear();
		for (int i = 0; i < nearCells.Count; i++)
		{
			CheckCollapseFlyingRoofAtAndAdjInternal(nearCells[i], map, removalMode, canRemoveThickRoof);
		}
		visitedCells.Clear();
	}

	public static void CheckCollapseFlyingRoofs(CellRect nearRect, Map map, bool removalMode = false, bool canRemoveThickRoof = false)
	{
		visitedCells.Clear();
		foreach (IntVec3 item in nearRect)
		{
			CheckCollapseFlyingRoofAtAndAdjInternal(item, map, removalMode, canRemoveThickRoof);
		}
		visitedCells.Clear();
	}

	private static bool CheckCollapseFlyingRoofAtAndAdjInternal(IntVec3 root, Map map, bool removalMode, bool canRemoveThickRoof)
	{
		RoofCollapseBuffer roofCollapseBuffer = map.roofCollapseBuffer;
		if (removalMode && roofCollapseBuffer.CellsMarkedToCollapse.Count > 0)
		{
			map.roofCollapseBufferResolver.CollapseRoofsMarkedToCollapse();
		}
		for (int i = 0; i < 5; i++)
		{
			IntVec3 intVec = root + GenAdj.CardinalDirectionsAndInside[i];
			if (!intVec.InBounds(map) || !intVec.Roofed(map) || visitedCells.Contains(intVec) || roofCollapseBuffer.IsMarkedToCollapse(intVec) || ConnectsToRoofHolder(intVec, map, visitedCells))
			{
				continue;
			}
			map.floodFiller.FloodFill(intVec, (IntVec3 x) => x.Roofed(map), delegate(IntVec3 x)
			{
				roofCollapseBuffer.MarkToCollapse(x);
			});
			if (!removalMode)
			{
				continue;
			}
			List<IntVec3> cellsMarkedToCollapse = roofCollapseBuffer.CellsMarkedToCollapse;
			for (int num = cellsMarkedToCollapse.Count - 1; num >= 0; num--)
			{
				RoofDef roofDef = map.roofGrid.RoofAt(cellsMarkedToCollapse[num]);
				if (roofDef != null && (canRemoveThickRoof || roofDef.VanishOnCollapse))
				{
					map.roofGrid.SetRoof(cellsMarkedToCollapse[num], null);
					cellsMarkedToCollapse.RemoveAt(num);
				}
			}
		}
		return false;
	}

	public static bool ConnectsToRoofHolder(IntVec3 c, Map map, HashSet<IntVec3> visitedCells)
	{
		bool connected = false;
		map.floodFiller.FloodFill(c, (IntVec3 x) => x.Roofed(map) && !connected, delegate(IntVec3 x)
		{
			if (visitedCells.Contains(x))
			{
				connected = true;
			}
			else
			{
				visitedCells.Add(x);
				for (int i = 0; i < 5; i++)
				{
					IntVec3 c2 = x + GenAdj.CardinalDirectionsAndInside[i];
					if (c2.InBounds(map))
					{
						Building edifice = c2.GetEdifice(map);
						if (edifice != null && edifice.def.holdsRoof)
						{
							connected = true;
							break;
						}
					}
				}
			}
		});
		return connected;
	}
}
