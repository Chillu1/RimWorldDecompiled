using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_FloorFill : SketchResolver
{
	private static HashSet<IntVec3> tmpWalls = new HashSet<IntVec3>();

	private static HashSet<IntVec3> tmpVisited = new HashSet<IntVec3>();

	private static Stack<Pair<int, int>> tmpStack = new Stack<Pair<int, int>>();

	private static List<IntVec3> tmpCells = new List<IntVec3>();

	protected override void ResolveInt(SketchResolveParams parms)
	{
		CellRect outerRect = parms.rect ?? parms.sketch.OccupiedRect;
		if (!TryFindFloors(out var floor, out var floor2, parms))
		{
			return;
		}
		bool valueOrDefault = parms.floorFillRoomsOnly == true;
		bool valueOrDefault2 = parms.singleFloorType == true;
		if (valueOrDefault)
		{
			tmpWalls.Clear();
			for (int i = 0; i < parms.sketch.Things.Count; i++)
			{
				SketchThing sketchThing = parms.sketch.Things[i];
				if (sketchThing.def.passability != Traversability.Impassable || sketchThing.def.Fillage != FillCategory.Full)
				{
					continue;
				}
				foreach (IntVec3 item in sketchThing.OccupiedRect)
				{
					tmpWalls.Add(item);
				}
			}
			tmpVisited.Clear();
			{
				foreach (IntVec3 item2 in outerRect)
				{
					if (!tmpWalls.Contains(item2))
					{
						FloorFillRoom(item2, tmpWalls, tmpVisited, parms.sketch, floor, floor2, outerRect, valueOrDefault2);
					}
				}
				return;
			}
		}
		bool[,] array = AbstractShapeGenerator.Generate(outerRect.Width, outerRect.Height, horizontalSymmetry: true, verticalSymmetry: true);
		foreach (IntVec3 item3 in outerRect)
		{
			if (!parms.sketch.ThingsAt(item3).Any((SketchThing x) => x.def.Fillage == FillCategory.Full))
			{
				if (array[item3.x - outerRect.minX, item3.z - outerRect.minZ] || valueOrDefault2)
				{
					parms.sketch.AddTerrain(floor, item3, wipeIfCollides: false);
				}
				else
				{
					parms.sketch.AddTerrain(floor2, item3, wipeIfCollides: false);
				}
			}
		}
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		if (!TryFindFloors(out var _, out var _, parms))
		{
			return false;
		}
		return true;
	}

	private static bool TryFindFloors(out TerrainDef floor1, out TerrainDef floor2, SketchResolveParams parms)
	{
		Predicate<TerrainDef> validator = (TerrainDef x) => SketchGenUtility.IsFloorAllowed(x, parms.allowWood ?? true, parms.allowConcrete ?? true, parms.useOnlyStonesAvailableOnMap, parms.onlyBuildableByPlayer == true, parms.onlyStoneFloors ?? true);
		if (!BaseGenUtility.TryRandomInexpensiveFloor(out floor1, validator))
		{
			floor2 = null;
			return false;
		}
		if (parms.singleFloorType == true)
		{
			floor2 = null;
			return true;
		}
		TerrainDef floor1Local = floor1;
		return BaseGenUtility.TryRandomInexpensiveFloor(out floor2, (TerrainDef x) => x != floor1Local && (validator == null || validator(x)));
	}

	private void FloorFillRoom(IntVec3 c, HashSet<IntVec3> walls, HashSet<IntVec3> visited, Sketch sketch, TerrainDef def1, TerrainDef def2, CellRect outerRect, bool singleFloorType)
	{
		if (visited.Contains(c))
		{
			return;
		}
		tmpCells.Clear();
		tmpStack.Clear();
		tmpStack.Push(new Pair<int, int>(c.x, c.z));
		visited.Add(c);
		int num = c.x;
		int num2 = c.x;
		int num3 = c.z;
		int num4 = c.z;
		while (tmpStack.Count != 0)
		{
			Pair<int, int> pair = tmpStack.Pop();
			int first = pair.First;
			int second = pair.Second;
			tmpCells.Add(new IntVec3(first, 0, second));
			num = Mathf.Min(num, first);
			num2 = Mathf.Max(num2, first);
			num3 = Mathf.Min(num3, second);
			num4 = Mathf.Max(num4, second);
			if (first > outerRect.minX && !walls.Contains(new IntVec3(first - 1, 0, second)) && !visited.Contains(new IntVec3(first - 1, 0, second)))
			{
				visited.Add(new IntVec3(first - 1, 0, second));
				tmpStack.Push(new Pair<int, int>(first - 1, second));
			}
			if (second > outerRect.minZ && !walls.Contains(new IntVec3(first, 0, second - 1)) && !visited.Contains(new IntVec3(first, 0, second - 1)))
			{
				visited.Add(new IntVec3(first, 0, second - 1));
				tmpStack.Push(new Pair<int, int>(first, second - 1));
			}
			if (first < outerRect.maxX && !walls.Contains(new IntVec3(first + 1, 0, second)) && !visited.Contains(new IntVec3(first + 1, 0, second)))
			{
				visited.Add(new IntVec3(first + 1, 0, second));
				tmpStack.Push(new Pair<int, int>(first + 1, second));
			}
			if (second < outerRect.maxZ && !walls.Contains(new IntVec3(first, 0, second + 1)) && !visited.Contains(new IntVec3(first, 0, second + 1)))
			{
				visited.Add(new IntVec3(first, 0, second + 1));
				tmpStack.Push(new Pair<int, int>(first, second + 1));
			}
		}
		for (int i = 0; i < tmpCells.Count; i++)
		{
			if (outerRect.IsOnEdge(tmpCells[i]))
			{
				return;
			}
		}
		CellRect cellRect = CellRect.FromLimits(num, num3, num2, num4);
		bool[,] array = AbstractShapeGenerator.Generate(cellRect.Width, cellRect.Height, horizontalSymmetry: true, verticalSymmetry: true);
		for (int j = 0; j < tmpCells.Count; j++)
		{
			IntVec3 pos = tmpCells[j];
			if (!sketch.ThingsAt(pos).Any((SketchThing x) => x.def.passability == Traversability.Impassable && x.def.Fillage == FillCategory.Full))
			{
				if (array[pos.x - cellRect.minX, pos.z - cellRect.minZ] || singleFloorType)
				{
					sketch.AddTerrain(def1, pos, wipeIfCollides: false);
				}
				else
				{
					sketch.AddTerrain(def2, pos, wipeIfCollides: false);
				}
			}
		}
	}
}
