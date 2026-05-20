using System;
using System.Collections.Generic;

namespace Verse;

public class RegionMaker
{
	private Map map;

	private Region newReg;

	private List<IntVec3> newRegCells = new List<IntVec3>();

	private bool working;

	private HashSet<IntVec3>[] linksProcessedAt = new HashSet<IntVec3>[4]
	{
		new HashSet<IntVec3>(),
		new HashSet<IntVec3>(),
		new HashSet<IntVec3>(),
		new HashSet<IntVec3>()
	};

	private RegionGrid regionGrid;

	private static HashSet<Thing> tmpProcessedThings = new HashSet<Thing>();

	public RegionMaker(Map map)
	{
		this.map = map;
	}

	public Region TryGenerateRegionFrom(IntVec3 root)
	{
		RegionType expectedRegionType = root.GetExpectedRegionType(map);
		if (expectedRegionType == RegionType.None)
		{
			return null;
		}
		if (working)
		{
			Log.Error("Trying to generate a new region but we are currently generating one. Nested calls are not allowed.");
			return null;
		}
		working = true;
		try
		{
			regionGrid = map.regionGrid;
			newReg = Region.MakeNewUnfilled(root, map);
			newReg.type = expectedRegionType;
			if (newReg.type == RegionType.Portal)
			{
				newReg.door = root.GetDoor(map);
			}
			FloodFillAndAddCells(root);
			CreateLinks();
			RegisterThingsInRegionListers();
			return newReg;
		}
		finally
		{
			working = false;
		}
	}

	private void FloodFillAndAddCells(IntVec3 root)
	{
		newRegCells.Clear();
		if (newReg.type.IsOneCellRegion())
		{
			AddCell(root);
			return;
		}
		map.floodFiller.FloodFill(root, (Predicate<IntVec3>)((IntVec3 x) => newReg.extentsLimit.Contains(x) && x.GetExpectedRegionType(map) == newReg.type), (Action<IntVec3>)AddCell, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
	}

	private void AddCell(IntVec3 c)
	{
		regionGrid.SetRegionAt(c, newReg);
		newRegCells.Add(c);
		if (newReg.extentsClose.minX > c.x)
		{
			newReg.extentsClose.minX = c.x;
		}
		if (newReg.extentsClose.maxX < c.x)
		{
			newReg.extentsClose.maxX = c.x;
		}
		if (newReg.extentsClose.minZ > c.z)
		{
			newReg.extentsClose.minZ = c.z;
		}
		if (newReg.extentsClose.maxZ < c.z)
		{
			newReg.extentsClose.maxZ = c.z;
		}
		if (c.x == 0 || c.x == map.Size.x - 1 || c.z == 0 || c.z == map.Size.z - 1)
		{
			newReg.touchesMapEdge = true;
		}
	}

	private void CreateLinks()
	{
		for (int i = 0; i < linksProcessedAt.Length; i++)
		{
			linksProcessedAt[i].Clear();
		}
		for (int j = 0; j < newRegCells.Count; j++)
		{
			IntVec3 c = newRegCells[j];
			SweepInTwoDirectionsAndTryToCreateLink(Rot4.North, c);
			SweepInTwoDirectionsAndTryToCreateLink(Rot4.South, c);
			SweepInTwoDirectionsAndTryToCreateLink(Rot4.East, c);
			SweepInTwoDirectionsAndTryToCreateLink(Rot4.West, c);
		}
	}

	private void SweepInTwoDirectionsAndTryToCreateLink(Rot4 potentialOtherRegionDir, IntVec3 c)
	{
		if (!potentialOtherRegionDir.IsValid)
		{
			return;
		}
		HashSet<IntVec3> hashSet = linksProcessedAt[potentialOtherRegionDir.AsInt];
		if (hashSet.Contains(c))
		{
			return;
		}
		IntVec3 c2 = c + potentialOtherRegionDir.FacingCell;
		if (c2.InBounds(map) && regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(c2) == newReg)
		{
			return;
		}
		RegionType expectedRegionType = c2.GetExpectedRegionType(map);
		if (expectedRegionType == RegionType.None)
		{
			return;
		}
		Rot4 rot = potentialOtherRegionDir;
		rot.Rotate(RotationDirection.Clockwise);
		int num = 0;
		int num2 = 0;
		hashSet.Add(c);
		if (!expectedRegionType.IsOneCellRegion())
		{
			while (true)
			{
				IntVec3 intVec = c + rot.FacingCell * (num + 1);
				if (!intVec.InBounds(map) || regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(intVec) != newReg || (intVec + potentialOtherRegionDir.FacingCell).GetExpectedRegionType(map) != expectedRegionType)
				{
					break;
				}
				if (!hashSet.Add(intVec))
				{
					Log.Error("We've processed the same cell twice.");
				}
				num++;
			}
			while (true)
			{
				IntVec3 intVec2 = c - rot.FacingCell * (num2 + 1);
				if (!intVec2.InBounds(map) || regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(intVec2) != newReg || (intVec2 + potentialOtherRegionDir.FacingCell).GetExpectedRegionType(map) != expectedRegionType)
				{
					break;
				}
				if (!hashSet.Add(intVec2))
				{
					Log.Error("We've processed the same cell twice.");
				}
				num2++;
			}
		}
		int length = num + num2 + 1;
		SpanDirection dir;
		IntVec3 root;
		if (potentialOtherRegionDir == Rot4.North)
		{
			dir = SpanDirection.East;
			root = c - rot.FacingCell * num2;
			root.z++;
		}
		else if (potentialOtherRegionDir == Rot4.South)
		{
			dir = SpanDirection.East;
			root = c + rot.FacingCell * num;
		}
		else if (potentialOtherRegionDir == Rot4.East)
		{
			dir = SpanDirection.North;
			root = c + rot.FacingCell * num;
			root.x++;
		}
		else
		{
			dir = SpanDirection.North;
			root = c - rot.FacingCell * num2;
		}
		EdgeSpan span = new EdgeSpan(root, dir, length);
		RegionLink regionLink = map.regionLinkDatabase.LinkFrom(span);
		regionLink.Register(newReg);
		newReg.links.Add(regionLink);
	}

	private void RegisterThingsInRegionListers()
	{
		CellRect extentsClose = newReg.extentsClose;
		extentsClose = extentsClose.ExpandedBy(1);
		extentsClose.ClipInsideMap(map);
		tmpProcessedThings.Clear();
		foreach (IntVec3 item in extentsClose)
		{
			bool flag = false;
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c = item + GenAdj.AdjacentCellsAndInside[i];
				if (c.InBounds(map) && regionGrid.GetValidRegionAt(c) == newReg)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				RegionListersUpdater.RegisterAllAt(item, map, tmpProcessedThings);
			}
		}
		tmpProcessedThings.Clear();
	}
}
