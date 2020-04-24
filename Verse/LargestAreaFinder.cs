using System;
using System.Collections.Generic;

namespace Verse
{
	public static class LargestAreaFinder
	{
		private static BoolGrid visited;

		private static List<IntVec3> randomOrderWorkingList = new List<IntVec3>();

		private static HashSet<IntVec3> tmpProcessed = new HashSet<IntVec3>();

		public static CellRect FindLargestRect(Map map, Predicate<IntVec3> predicate, int breakEarlyOn = -1)
		{
			if (visited == null)
			{
				visited = new BoolGrid(map);
			}
			visited.ClearAndResizeTo(map);
			Rand.PushState(map.uniqueID ^ 0x1CDAF373);
			CellRect largestRect = CellRect.Empty;
			for (int i = 0; i < 3; i++)
			{
				tmpProcessed.Clear();
				foreach (IntVec3 item in map.cellsInRandomOrder.GetAll().InRandomOrder(randomOrderWorkingList))
				{
					CellRect cellRect = FindLargestRectAt(item, map, tmpProcessed, predicate);
					if (cellRect.Area > largestRect.Area)
					{
						largestRect = cellRect;
						if (ShouldBreakEarly())
						{
							break;
						}
					}
				}
				if (ShouldBreakEarly())
				{
					break;
				}
			}
			Rand.PopState();
			return largestRect;
			bool ShouldBreakEarly()
			{
				if (breakEarlyOn >= 0 && largestRect.Width >= breakEarlyOn)
				{
					return largestRect.Height >= breakEarlyOn;
				}
				return false;
			}
		}

		private static CellRect FindLargestRectAt(IntVec3 c, Map map, HashSet<IntVec3> processed, Predicate<IntVec3> predicate)
		{
			if (processed.Contains(c) || !predicate(c))
			{
				return CellRect.Empty;
			}
			CellRect rect = CellRect.SingleCell(c);
			bool flag;
			do
			{
				flag = false;
				if (rect.Width <= rect.Height)
				{
					if (rect.maxX + 1 < map.Size.x && CanExpand(Rot4.East))
					{
						rect.maxX++;
						flag = true;
					}
					if (rect.minX > 0 && CanExpand(Rot4.West))
					{
						rect.minX--;
						flag = true;
					}
				}
				if (rect.Height <= rect.Width)
				{
					if (rect.maxZ + 1 < map.Size.z && CanExpand(Rot4.North))
					{
						rect.maxZ++;
						flag = true;
					}
					if (rect.minZ > 0 && CanExpand(Rot4.South))
					{
						rect.minZ--;
						flag = true;
					}
				}
			}
			while (flag);
			foreach (IntVec3 item in rect)
			{
				processed.Add(item);
			}
			return rect;
			bool CanExpand(Rot4 dir)
			{
				foreach (IntVec3 edgeCell in rect.GetEdgeCells(dir))
				{
					IntVec3 intVec = edgeCell + dir.FacingCell;
					if (processed.Contains(intVec) || !predicate(intVec))
					{
						return false;
					}
				}
				return true;
			}
		}
	}
}
