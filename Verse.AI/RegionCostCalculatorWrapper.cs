using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public class RegionCostCalculatorWrapper
	{
		private Map map;

		private IntVec3 endCell;

		private HashSet<Region> destRegions = new HashSet<Region>();

		private int moveTicksCardinal;

		private int moveTicksDiagonal;

		private RegionCostCalculator regionCostCalculator;

		private Region cachedRegion;

		private RegionLink cachedBestLink;

		private RegionLink cachedSecondBestLink;

		private int cachedBestLinkCost;

		private int cachedSecondBestLinkCost;

		private bool cachedRegionIsDestination;

		private Region[] regionGrid;

		public RegionCostCalculatorWrapper(Map map)
		{
			this.map = map;
			regionCostCalculator = new RegionCostCalculator(map);
		}

		public void Init(CellRect end, TraverseParms traverseParms, int moveTicksCardinal, int moveTicksDiagonal, ByteGrid avoidGrid, Area allowedArea, bool drafted, List<int> disallowedCorners)
		{
			this.moveTicksCardinal = moveTicksCardinal;
			this.moveTicksDiagonal = moveTicksDiagonal;
			endCell = end.CenterCell;
			cachedRegion = null;
			cachedBestLink = null;
			cachedSecondBestLink = null;
			cachedBestLinkCost = 0;
			cachedSecondBestLinkCost = 0;
			cachedRegionIsDestination = false;
			regionGrid = map.regionGrid.DirectGrid;
			destRegions.Clear();
			if (end.Width == 1 && end.Height == 1)
			{
				Region region = endCell.GetRegion(map);
				if (region != null)
				{
					destRegions.Add(region);
				}
			}
			else
			{
				foreach (IntVec3 item in end)
				{
					if (item.InBounds(map) && !disallowedCorners.Contains(map.cellIndices.CellToIndex(item)))
					{
						Region region2 = item.GetRegion(map);
						if (region2 != null && region2.Allows(traverseParms, isDestination: true))
						{
							destRegions.Add(region2);
						}
					}
				}
			}
			if (destRegions.Count == 0)
			{
				Log.Error("Couldn't find any destination regions. This shouldn't ever happen because we've checked reachability.");
			}
			regionCostCalculator.Init(end, destRegions, traverseParms, moveTicksCardinal, moveTicksDiagonal, avoidGrid, allowedArea, drafted);
		}

		public int GetPathCostFromDestToRegion(int cellIndex)
		{
			Region region = regionGrid[cellIndex];
			IntVec3 cell = map.cellIndices.IndexToCell(cellIndex);
			if (region != cachedRegion)
			{
				cachedRegionIsDestination = destRegions.Contains(region);
				if (cachedRegionIsDestination)
				{
					return OctileDistanceToEnd(cell);
				}
				cachedBestLinkCost = regionCostCalculator.GetRegionBestDistances(region, out cachedBestLink, out cachedSecondBestLink, out cachedSecondBestLinkCost);
				cachedRegion = region;
			}
			else if (cachedRegionIsDestination)
			{
				return OctileDistanceToEnd(cell);
			}
			if (cachedBestLink != null)
			{
				int num = regionCostCalculator.RegionLinkDistance(cell, cachedBestLink, 1);
				int num3;
				if (cachedSecondBestLink != null)
				{
					int num2 = regionCostCalculator.RegionLinkDistance(cell, cachedSecondBestLink, 1);
					num3 = Mathf.Min(cachedSecondBestLinkCost + num2, cachedBestLinkCost + num);
				}
				else
				{
					num3 = cachedBestLinkCost + num;
				}
				return num3 + OctileDistanceToEndEps(cell);
			}
			return 10000;
		}

		private int OctileDistanceToEnd(IntVec3 cell)
		{
			int dx = Mathf.Abs(cell.x - endCell.x);
			int dz = Mathf.Abs(cell.z - endCell.z);
			return GenMath.OctileDistance(dx, dz, moveTicksCardinal, moveTicksDiagonal);
		}

		private int OctileDistanceToEndEps(IntVec3 cell)
		{
			int dx = Mathf.Abs(cell.x - endCell.x);
			int dz = Mathf.Abs(cell.z - endCell.z);
			return GenMath.OctileDistance(dx, dz, 2, 3);
		}
	}
}
