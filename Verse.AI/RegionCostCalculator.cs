using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI;

public class RegionCostCalculator
{
	private struct RegionLinkQueueEntry
	{
		private readonly Region from;

		private readonly RegionLink link;

		private readonly int cost;

		private readonly int estimatedPathCost;

		public Region From => from;

		public RegionLink Link => link;

		public int Cost => cost;

		public int EstimatedPathCost => estimatedPathCost;

		public RegionLinkQueueEntry(Region from, RegionLink link, int cost, int estimatedPathCost)
		{
			this.from = from;
			this.link = link;
			this.cost = cost;
			this.estimatedPathCost = estimatedPathCost;
		}
	}

	private class DistanceComparer : IComparer<RegionLinkQueueEntry>
	{
		public int Compare(RegionLinkQueueEntry a, RegionLinkQueueEntry b)
		{
			return a.EstimatedPathCost.CompareTo(b.EstimatedPathCost);
		}
	}

	private readonly Map map;

	private Region[] regionGrid;

	private TraverseParms traverseParms;

	private PathingContext pathingContext;

	private IntVec3 destinationCell;

	private float moveTicksCardinal;

	private float moveTicksDiagonal;

	private ByteGrid avoidGrid;

	private Area allowedArea;

	private bool drafted;

	private readonly Func<int, int, float> preciseRegionLinkDistancesDistanceGetter;

	private readonly Dictionary<int, RegionLink> regionMinLink = new Dictionary<int, RegionLink>();

	private readonly Dictionary<RegionLink, int> distances = new Dictionary<RegionLink, int>();

	private readonly FastPriorityQueue<RegionLinkQueueEntry> queue = new FastPriorityQueue<RegionLinkQueueEntry>(new DistanceComparer());

	private readonly Dictionary<Region, int> minPathCosts = new Dictionary<Region, int>();

	private readonly List<Pair<RegionLink, int>> preciseRegionLinkDistances = new List<Pair<RegionLink, int>>();

	private readonly Dictionary<RegionLink, IntVec3> linkTargetCells = new Dictionary<RegionLink, IntVec3>();

	private const int SampleCount = 11;

	private static readonly int[] pathCostSamples = new int[11];

	private static readonly List<int> tmpCellIndices = new List<int>();

	private static readonly Dictionary<int, float> tmpDistances = new Dictionary<int, float>();

	private static readonly List<int> tmpPathableNeighborIndices = new List<int>();

	public RegionCostCalculator(Map map)
	{
		this.map = map;
		preciseRegionLinkDistancesDistanceGetter = PreciseRegionLinkDistancesDistanceGetter;
	}

	public void Init(CellRect destination, HashSet<Region> destRegions, TraverseParms parms, float moveTicksCardinal, float moveTicksDiagonal, ByteGrid avoidGrid, Area allowedArea, bool drafted)
	{
		regionGrid = map.regionGrid.DirectGrid;
		traverseParms = parms;
		pathingContext = map.pathing.For(parms);
		destinationCell = destination.CenterCell;
		this.moveTicksCardinal = moveTicksCardinal;
		this.moveTicksDiagonal = moveTicksDiagonal;
		this.avoidGrid = avoidGrid;
		this.allowedArea = allowedArea;
		this.drafted = drafted;
		regionMinLink.Clear();
		distances.Clear();
		linkTargetCells.Clear();
		queue.Clear();
		minPathCosts.Clear();
		foreach (Region destRegion in destRegions)
		{
			int minPathCost = RegionMedianPathCost(destRegion);
			for (int i = 0; i < destRegion.links.Count; i++)
			{
				RegionLink regionLink = destRegion.links[i];
				if (!regionLink.GetOtherRegion(destRegion).Allows(traverseParms, isDestination: false))
				{
					continue;
				}
				int num = RegionLinkDistance(destinationCell, regionLink, minPathCost);
				if (distances.TryGetValue(regionLink, out var value))
				{
					if (num < value)
					{
						linkTargetCells[regionLink] = GetLinkTargetCell(destinationCell, regionLink);
					}
					num = Math.Min(value, num);
				}
				else
				{
					linkTargetCells[regionLink] = GetLinkTargetCell(destinationCell, regionLink);
				}
				distances[regionLink] = num;
			}
			GetPreciseRegionLinkDistances(destRegion, destination, preciseRegionLinkDistances);
			for (int j = 0; j < preciseRegionLinkDistances.Count; j++)
			{
				Pair<RegionLink, int> pair = preciseRegionLinkDistances[j];
				RegionLink first = pair.First;
				int num2 = distances[first];
				int num3;
				if (pair.Second > num2)
				{
					distances[first] = pair.Second;
					num3 = pair.Second;
				}
				else
				{
					num3 = num2;
				}
				queue.Push(new RegionLinkQueueEntry(destRegion, first, num3, num3));
			}
		}
	}

	public int GetRegionDistance(Region region, out RegionLink minLink)
	{
		if (regionMinLink.TryGetValue(region.id, out minLink))
		{
			return distances[minLink];
		}
		while (queue.Count != 0)
		{
			RegionLinkQueueEntry regionLinkQueueEntry = queue.Pop();
			int num = distances[regionLinkQueueEntry.Link];
			if (regionLinkQueueEntry.Cost != num)
			{
				continue;
			}
			Region otherRegion = regionLinkQueueEntry.Link.GetOtherRegion(regionLinkQueueEntry.From);
			if (otherRegion == null || !otherRegion.valid)
			{
				continue;
			}
			int num2 = 0;
			if (otherRegion.door != null)
			{
				num2 = PathUtility.GetBuildingCost(otherRegion.door, traverseParms, traverseParms.pawn, null);
				if (num2 == 65535)
				{
					continue;
				}
				num2 += OctileDistance(1, 0);
			}
			int minPathCost = RegionMedianPathCost(otherRegion);
			for (int i = 0; i < otherRegion.links.Count; i++)
			{
				RegionLink regionLink = otherRegion.links[i];
				if (regionLink == regionLinkQueueEntry.Link || !regionLink.GetOtherRegion(otherRegion).Allows(traverseParms, isDestination: false))
				{
					continue;
				}
				int val = ((otherRegion.door != null) ? num2 : RegionLinkDistance(regionLinkQueueEntry.Link, regionLink, minPathCost));
				val = Math.Max(val, 1);
				int num3 = num + val;
				int estimatedPathCost = MinimumRegionLinkDistance(destinationCell, regionLink) + num3;
				if (distances.TryGetValue(regionLink, out var value))
				{
					if (num3 < value)
					{
						distances[regionLink] = num3;
						queue.Push(new RegionLinkQueueEntry(otherRegion, regionLink, num3, estimatedPathCost));
					}
				}
				else
				{
					distances.Add(regionLink, num3);
					queue.Push(new RegionLinkQueueEntry(otherRegion, regionLink, num3, estimatedPathCost));
				}
			}
			if (!regionMinLink.ContainsKey(otherRegion.id))
			{
				regionMinLink.Add(otherRegion.id, regionLinkQueueEntry.Link);
				if (otherRegion == region)
				{
					minLink = regionLinkQueueEntry.Link;
					return regionLinkQueueEntry.Cost;
				}
			}
		}
		return 10000;
	}

	public int GetRegionBestDistances(Region region, out RegionLink bestLink, out RegionLink secondBestLink, out int secondBestCost)
	{
		int regionDistance = GetRegionDistance(region, out bestLink);
		secondBestLink = null;
		secondBestCost = int.MaxValue;
		for (int i = 0; i < region.links.Count; i++)
		{
			RegionLink regionLink = region.links[i];
			if (regionLink != bestLink && regionLink.GetOtherRegion(region).Allows(traverseParms, isDestination: false) && distances.TryGetValue(regionLink, out var value) && value < secondBestCost)
			{
				secondBestCost = value;
				secondBestLink = regionLink;
			}
		}
		return regionDistance;
	}

	public int RegionMedianPathCost(Region region)
	{
		if (minPathCosts.TryGetValue(region, out var value))
		{
			return value;
		}
		bool ignoreAllowedAreaCost = allowedArea != null && region.OverlapWith(allowedArea) != AreaOverlap.None;
		CellIndices cellIndices = map.cellIndices;
		Rand.PushState();
		Rand.Seed = cellIndices.CellToIndex(region.extentsClose.CenterCell) * (region.links.Count + 1);
		for (int i = 0; i < 11; i++)
		{
			pathCostSamples[i] = GetCellCostFast(cellIndices.CellToIndex(region.RandomCell), ignoreAllowedAreaCost);
		}
		Rand.PopState();
		Array.Sort(pathCostSamples);
		return minPathCosts[region] = pathCostSamples[4];
	}

	private int GetCellCostFast(int index, bool ignoreAllowedAreaCost = false)
	{
		int num = pathingContext.pathGrid.Grid_Unsafe[index];
		if (avoidGrid != null)
		{
			num += avoidGrid[index] * 8;
		}
		if (allowedArea != null && !ignoreAllowedAreaCost && !allowedArea[index])
		{
			num += 600;
		}
		if (drafted)
		{
			return num + map.terrainGrid.topGrid[index].extraDraftedPerceivedPathCost;
		}
		return num + map.terrainGrid.topGrid[index].extraNonDraftedPerceivedPathCost;
	}

	private int RegionLinkDistance(RegionLink a, RegionLink b, int minPathCost)
	{
		IntVec3 value;
		IntVec3 obj = (linkTargetCells.TryGetValue(a, out value) ? value : RegionLinkCenter(a));
		IntVec3 value2;
		IntVec3 intVec = (linkTargetCells.TryGetValue(b, out value2) ? value2 : RegionLinkCenter(b));
		IntVec3 intVec2 = obj - intVec;
		int num = Math.Abs(intVec2.x);
		int num2 = Math.Abs(intVec2.z);
		return OctileDistance(num, num2) + minPathCost * Math.Max(num, num2) + minPathCost * Math.Min(num, num2);
	}

	public int RegionLinkDistance(IntVec3 cell, RegionLink link, int minPathCost)
	{
		IntVec3 linkTargetCell = GetLinkTargetCell(cell, link);
		IntVec3 intVec = cell - linkTargetCell;
		int num = Math.Abs(intVec.x);
		int num2 = Math.Abs(intVec.z);
		return OctileDistance(num, num2) + minPathCost * Math.Max(num, num2) + minPathCost * Math.Min(num, num2);
	}

	private static int SpanCenterX(EdgeSpan e)
	{
		return e.root.x + ((e.dir == SpanDirection.East) ? (e.length / 2) : 0);
	}

	private static int SpanCenterZ(EdgeSpan e)
	{
		return e.root.z + ((e.dir == SpanDirection.North) ? (e.length / 2) : 0);
	}

	private static IntVec3 RegionLinkCenter(RegionLink link)
	{
		return new IntVec3(SpanCenterX(link.span), 0, SpanCenterZ(link.span));
	}

	private int MinimumRegionLinkDistance(IntVec3 cell, RegionLink link)
	{
		IntVec3 intVec = cell - LinkClosestCell(cell, link);
		return OctileDistance(Math.Abs(intVec.x), Math.Abs(intVec.z));
	}

	private int OctileDistance(int dx, int dz)
	{
		return GenMath.OctileDistance(dx, dz, Mathf.RoundToInt(moveTicksCardinal), Mathf.RoundToInt(moveTicksDiagonal));
	}

	private IntVec3 GetLinkTargetCell(IntVec3 cell, RegionLink link)
	{
		return LinkClosestCell(cell, link);
	}

	private static IntVec3 LinkClosestCell(IntVec3 cell, RegionLink link)
	{
		EdgeSpan span = link.span;
		int num = 0;
		int num2 = 0;
		if (span.dir == SpanDirection.North)
		{
			num2 = span.length - 1;
		}
		else
		{
			num = span.length - 1;
		}
		IntVec3 root = span.root;
		return new IntVec3(Mathf.Clamp(cell.x, root.x, root.x + num), 0, Mathf.Clamp(cell.z, root.z, root.z + num2));
	}

	private void GetPreciseRegionLinkDistances(Region region, CellRect destination, List<Pair<RegionLink, int>> outDistances)
	{
		outDistances.Clear();
		tmpCellIndices.Clear();
		if (destination.Width == 1 && destination.Height == 1)
		{
			tmpCellIndices.Add(map.cellIndices.CellToIndex(destination.CenterCell));
		}
		else
		{
			foreach (IntVec3 item in destination)
			{
				if (item.InBounds(map))
				{
					tmpCellIndices.Add(map.cellIndices.CellToIndex(item));
				}
			}
		}
		Dijkstra<int>.Run(tmpCellIndices, (int x) => PreciseRegionLinkDistancesNeighborsGetter(x, region), preciseRegionLinkDistancesDistanceGetter, tmpDistances);
		for (int num = 0; num < region.links.Count; num++)
		{
			RegionLink regionLink = region.links[num];
			if (regionLink.GetOtherRegion(region).Allows(traverseParms, isDestination: false))
			{
				if (!tmpDistances.TryGetValue(map.cellIndices.CellToIndex(linkTargetCells[regionLink]), out var value))
				{
					Log.ErrorOnce("Dijkstra couldn't reach one of the cells even though they are in the same region. There is most likely something wrong with the neighbor nodes getter.", 1938471531);
					value = 100f;
				}
				outDistances.Add(new Pair<RegionLink, int>(regionLink, (int)value));
			}
		}
	}

	private IEnumerable<int> PreciseRegionLinkDistancesNeighborsGetter(int node, Region region)
	{
		if (regionGrid[node] == null || regionGrid[node] != region)
		{
			return null;
		}
		return PathableNeighborIndices(node);
	}

	private float PreciseRegionLinkDistancesDistanceGetter(int a, int b)
	{
		return (float)GetCellCostFast(b) + (AreCellsDiagonal(a, b) ? moveTicksDiagonal : moveTicksCardinal);
	}

	private bool AreCellsDiagonal(int a, int b)
	{
		int x = map.Size.x;
		if (a % x != b % x)
		{
			return a / x != b / x;
		}
		return false;
	}

	private List<int> PathableNeighborIndices(int index)
	{
		tmpPathableNeighborIndices.Clear();
		PathGrid pathGrid = pathingContext.pathGrid;
		int x = map.Size.x;
		bool num = index % x > 0;
		bool flag = index % x < x - 1;
		bool flag2 = index >= x;
		bool flag3 = index / x < map.Size.z - 1;
		if (flag2 && pathGrid.WalkableFast(index - x))
		{
			tmpPathableNeighborIndices.Add(index - x);
		}
		if (flag && pathGrid.WalkableFast(index + 1))
		{
			tmpPathableNeighborIndices.Add(index + 1);
		}
		if (num && pathGrid.WalkableFast(index - 1))
		{
			tmpPathableNeighborIndices.Add(index - 1);
		}
		if (flag3 && pathGrid.WalkableFast(index + x))
		{
			tmpPathableNeighborIndices.Add(index + x);
		}
		bool canBashFences = traverseParms.canBashFences;
		bool flag4 = !num || PathUtility.BlocksDiagonalMovement(index - 1, pathingContext, canBashFences);
		bool flag5 = !flag || PathUtility.BlocksDiagonalMovement(index + 1, pathingContext, canBashFences);
		if (flag2 && !PathUtility.BlocksDiagonalMovement(index - x, pathingContext, canBashFences))
		{
			if (!flag5 && pathGrid.WalkableFast(index - x + 1))
			{
				tmpPathableNeighborIndices.Add(index - x + 1);
			}
			if (!flag4 && pathGrid.WalkableFast(index - x - 1))
			{
				tmpPathableNeighborIndices.Add(index - x - 1);
			}
		}
		if (flag3 && !PathUtility.BlocksDiagonalMovement(index + x, pathingContext, canBashFences))
		{
			if (!flag5 && pathGrid.WalkableFast(index + x + 1))
			{
				tmpPathableNeighborIndices.Add(index + x + 1);
			}
			if (!flag4 && pathGrid.WalkableFast(index + x - 1))
			{
				tmpPathableNeighborIndices.Add(index + x - 1);
			}
		}
		return tmpPathableNeighborIndices;
	}
}
