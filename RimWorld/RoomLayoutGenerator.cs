using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Utility;

namespace RimWorld;

public static class RoomLayoutGenerator
{
	private static readonly IntRange DefaultMaxMergedRoomsRange = new IntRange(1, 3);

	private static readonly List<CellRect> tmpRoomRects = new List<CellRect>();

	private static readonly List<CellRect> tmpCorridors = new List<CellRect>();

	private static readonly List<CellRect> tmpSpaces = new List<CellRect>();

	private static readonly List<List<CellRect>> tmpMergedRoomRects = new List<List<CellRect>>();

	private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

	private static readonly List<CorridorShape> tmpShapes = new List<CorridorShape>();

	public static StructureLayout GenerateRandomLayout(LayoutStructureSketch sketch, CellRect container, int minRoomWidth = 6, int minRoomHeight = 6, float areaPrunePercent = 0.2f, bool canRemoveRooms = false, bool generateDoors = true, LayoutRoomDef corridor = null, int corridorExpansion = 2, IntRange? maxMergeRoomsRange = null, CorridorShape corridorShapes = CorridorShape.All, bool canDisconnectRooms = true, bool singleRoom = false)
	{
		return GenerateRoomLayout(new RoomLayoutParams
		{
			sketch = sketch,
			container = container,
			areaPrunePercent = areaPrunePercent,
			minRoomHeight = Mathf.Max(minRoomWidth, 3),
			minRoomWidth = Mathf.Max(minRoomHeight, 3),
			canRemoveRooms = canRemoveRooms,
			generateDoors = generateDoors,
			corridor = corridor,
			corridorExpansion = corridorExpansion,
			maxMergeRoomsRange = maxMergeRoomsRange,
			corridorShapes = corridorShapes,
			canDisconnectRooms = canDisconnectRooms,
			singleRoom = singleRoom
		});
	}

	public static StructureLayout GenerateRandomLayout(CellRect container, int minRoomWidth = 6, int minRoomHeight = 6, float areaPrunePercent = 0.2f, bool canRemoveRooms = false, bool generateDoors = true, LayoutRoomDef corridor = null, int corridorExpansion = 2, IntRange? maxMergeRoomsRange = null, CorridorShape corridorShapes = CorridorShape.All, bool canDisconnectRooms = true, bool singleRoom = false)
	{
		return GenerateRoomLayout(new RoomLayoutParams
		{
			container = container,
			areaPrunePercent = areaPrunePercent,
			minRoomHeight = Mathf.Max(minRoomWidth, 3),
			minRoomWidth = Mathf.Max(minRoomHeight, 3),
			canRemoveRooms = canRemoveRooms,
			generateDoors = generateDoors,
			corridor = corridor,
			corridorExpansion = corridorExpansion,
			maxMergeRoomsRange = maxMergeRoomsRange,
			corridorShapes = corridorShapes,
			canDisconnectRooms = canDisconnectRooms,
			singleRoom = singleRoom
		});
	}

	private static StructureLayout GenerateRoomLayout(RoomLayoutParams parms)
	{
		StructureLayout structureLayout = new StructureLayout(parms.sketch, parms.container);
		if (parms.singleRoom)
		{
			structureLayout.AddRoom(new List<CellRect> { parms.container });
		}
		else if (parms.corridor == null)
		{
			SplitRandom(parms.container, tmpRoomRects, parms.minRoomWidth, parms.minRoomHeight);
			MergeRandom(tmpRoomRects, tmpMergedRoomRects, parms.maxMergeRoomsRange ?? DefaultMaxMergedRoomsRange);
			foreach (List<CellRect> tmpMergedRoomRect in tmpMergedRoomRects)
			{
				structureLayout.AddRoom(tmpMergedRoomRect);
			}
		}
		else
		{
			SplitCorridor(parms.container, tmpCorridors, tmpSpaces, parms.corridorExpansion, parms.minRoomWidth, parms.minRoomHeight, parms.corridorShapes);
			MergeAddCorridors(parms, structureLayout);
			foreach (CellRect tmpSpace in tmpSpaces)
			{
				SplitRandom(tmpSpace, tmpRoomRects, parms.minRoomWidth, parms.minRoomHeight);
				MergeRandom(tmpRoomRects, tmpMergedRoomRects, parms.maxMergeRoomsRange ?? DefaultMaxMergedRoomsRange);
				foreach (List<CellRect> tmpMergedRoomRect2 in tmpMergedRoomRects)
				{
					structureLayout.AddRoom(tmpMergedRoomRect2);
				}
			}
		}
		float num = (float)structureLayout.Area - parms.areaPrunePercent * (float)structureLayout.Area;
		int num2 = 100;
		int num3 = 0;
		int num4 = structureLayout.Rooms.Count / 3;
		while (parms.canRemoveRooms && structureLayout.Rooms.Count > 4 && num3 < num4 && (float)structureLayout.Area > num && num2-- > 0)
		{
			LayoutRoom layoutRoom = structureLayout.Rooms.RandomElement();
			if (layoutRoom.requiredDef == null && (parms.canDisconnectRooms || structureLayout.CanRemoveWithoutDisconnection(layoutRoom)))
			{
				structureLayout.RemoveRoom(layoutRoom);
				num3++;
			}
		}
		num2 = 0;
		while ((float)structureLayout.Area > num && num2-- > 0)
		{
			structureLayout.TryMinimizeLayoutWithoutDisconnection();
		}
		structureLayout.FinalizeRooms();
		if (parms.generateDoors)
		{
			CreateDoors(structureLayout, parms.entranceCount);
		}
		return structureLayout;
	}

	private static void MergeAddCorridors(RoomLayoutParams parms, StructureLayout layout)
	{
		while (tmpCorridors.Any())
		{
			List<CellRect> list = new List<CellRect> { tmpCorridors[0] };
			tmpCorridors.RemoveAt(0);
			for (int num = tmpCorridors.Count - 1; num >= 0; num--)
			{
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					if (tmpCorridors[num].Overlaps(list[i]))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list.Add(tmpCorridors[num]);
					tmpCorridors.RemoveAt(num);
				}
			}
			LayoutRoom layoutRoom = layout.AddRoom(list);
			layoutRoom.requiredDef = parms.corridor;
			layoutRoom.noExteriorDoors = true;
		}
	}

	private static void CreateDoors(StructureLayout layout, int entranceCount)
	{
		HashSet<int> hashSet = new HashSet<int>();
		tmpCells.Clear();
		tmpCells.AddRange(layout.container.Cells.InRandomOrder());
		for (int i = 0; i < tmpCells.Count; i++)
		{
			IntVec3 intVec = tmpCells[i];
			if (!layout.IsWallAt(intVec))
			{
				continue;
			}
			if (layout.IsGoodForHorizontalDoor(intVec))
			{
				int roomIdAt = layout.GetRoomIdAt(intVec + IntVec3.North);
				int roomIdAt2 = layout.GetRoomIdAt(intVec + IntVec3.South);
				bool flag = layout.IsOutside(intVec + IntVec3.North) || layout.IsOutside(intVec + IntVec3.South);
				int item = Gen.HashOrderless(roomIdAt, roomIdAt2);
				if (hashSet.Contains(item) || (flag && entranceCount <= 0))
				{
					continue;
				}
				layout.Add(intVec, RoomLayoutCellType.Door);
				hashSet.Add(item);
				if (flag)
				{
					entranceCount--;
				}
			}
			if (!layout.IsGoodForVerticalDoor(intVec))
			{
				continue;
			}
			int roomIdAt3 = layout.GetRoomIdAt(intVec + IntVec3.East);
			int roomIdAt4 = layout.GetRoomIdAt(intVec + IntVec3.West);
			bool flag2 = layout.IsOutside(intVec + IntVec3.East) || layout.IsOutside(intVec + IntVec3.West);
			int item2 = Gen.HashOrderless(roomIdAt3, roomIdAt4);
			if (!hashSet.Contains(item2) && (!flag2 || entranceCount > 0))
			{
				layout.Add(intVec, RoomLayoutCellType.Door);
				hashSet.Add(item2);
				if (flag2)
				{
					entranceCount--;
				}
			}
		}
		tmpCells.Clear();
	}

	private static void SplitCorridor(CellRect rect, List<CellRect> corridors, List<CellRect> spaces, int expansion, int minRoomWidth, int minRoomHeight, CorridorShape shapes)
	{
		tmpCorridors.Clear();
		tmpShapes.Clear();
		spaces.Clear();
		foreach (CorridorShape bitFlag in shapes.GetBitFlags())
		{
			tmpShapes.Add(bitFlag);
		}
		IntVec3 xExpansion = new IntVec3(expansion, 0, 0);
		IntVec3 zExpansion = new IntVec3(0, 0, expansion);
		int xRange = Mathf.Max(rect.Width / 2 - minRoomWidth - expansion - 1, 0);
		int zRange = Mathf.Max(rect.Height / 2 - minRoomHeight - expansion - 1, 0);
		float num = ((rect.Width < rect.Height) ? ((float)rect.Width / (float)rect.Height) : ((float)rect.Height / (float)rect.Width));
		if (tmpShapes.Contains(CorridorShape.H) && (num < 0.5f || rect.Width - expansion * 2 - minRoomWidth * 3 - 2 < 0 || rect.Height - expansion * 2 - minRoomHeight * 3 - 2 < 0))
		{
			tmpShapes.Remove(CorridorShape.H);
		}
		if (tmpShapes.Empty())
		{
			Log.Error("Attempted to spawn a corridor with no valid options.");
			return;
		}
		CorridorShape corridorShape = tmpShapes.RandomElement();
		switch (corridorShape)
		{
		case CorridorShape.Straight:
			GenerateStraightCorridor(rect, corridors, spaces, zExpansion, xExpansion, xRange, zRange);
			break;
		case CorridorShape.Cross:
			GenerateCrossCorridor(rect, corridors, spaces, zExpansion, xExpansion, xRange, zRange);
			break;
		case CorridorShape.T:
			GenerateTCorridor(rect, corridors, spaces, zExpansion, xExpansion, xRange, zRange);
			break;
		case CorridorShape.H:
			GenerateHCorridor(rect, corridors, spaces, zExpansion, xExpansion, xRange, zRange);
			break;
		case CorridorShape.AsymmetricCross:
			GenerateAsymmetricCrossCorridor(rect, corridors, spaces, zExpansion, xExpansion, xRange, zRange);
			break;
		default:
			Log.Error($"Unhanlded corridor shape {corridorShape}");
			break;
		}
	}

	private static void GenerateStraightCorridor(CellRect rect, List<CellRect> corridors, List<CellRect> spaces, IntVec3 zExpansion, IntVec3 xExpansion, int xRange, int zRange)
	{
		Rot4 rot = Rot4.East;
		if (rect.Height > rect.Width)
		{
			rot = Rot4.North;
		}
		int offset = ((rot == Rot4.East) ? Rand.Range(-zRange, zRange) : Rand.Range(-xRange, xRange));
		IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite, offset);
		IntVec3 centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot, offset);
		CellRect cellRect = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z).ExpandedBy((rot == Rot4.East) ? zExpansion : xExpansion);
		corridors.Add(cellRect);
		DivideRectsByCorridor(rect, cellRect, spaces, rot);
	}

	private static void GenerateCrossCorridor(CellRect rect, List<CellRect> corridors, List<CellRect> spaces, IntVec3 zExpansion, IntVec3 xExpansion, int xRange, int zRange)
	{
		Rot4 east = Rot4.East;
		int offset = Rand.Range(-zRange, zRange);
		IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(east.Opposite, offset);
		IntVec3 centerCellOnEdge2 = rect.GetCenterCellOnEdge(east, offset);
		CellRect cellRect = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z).ExpandedBy((east == Rot4.East) ? zExpansion : xExpansion);
		corridors.Add(cellRect);
		DivideRectsByCorridor(rect, cellRect, spaces, east);
		east = Rot4.North;
		IntVec3 point = cellRect.CenterCell + new IntVec3(Rand.Range(-xRange, xRange), 0, 0);
		centerCellOnEdge = rect.GetCellOnEdge(east.Opposite, point);
		centerCellOnEdge2 = rect.GetCellOnEdge(east, point);
		CellRect cellRect2 = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, cellRect.minZ);
		CellRect cellRect3 = CellRect.FromLimits(centerCellOnEdge.x, cellRect.maxZ, centerCellOnEdge2.x, centerCellOnEdge2.z);
		cellRect2 = cellRect2.ExpandedBy((east == Rot4.East) ? zExpansion : xExpansion);
		cellRect3 = cellRect3.ExpandedBy((east == Rot4.East) ? zExpansion : xExpansion);
		corridors.Add(cellRect2);
		corridors.Add(cellRect3);
		DivideRectsByCorridor(spaces[(!spaces[0].Overlaps(cellRect2)) ? 1 : 0], cellRect2, spaces, east);
		DivideRectsByCorridor(spaces[(!spaces[0].Overlaps(cellRect3)) ? 1 : 0], cellRect3, spaces, east);
		spaces.RemoveAt(0);
		spaces.RemoveAt(0);
	}

	private static void GenerateAsymmetricCrossCorridor(CellRect rect, List<CellRect> corridors, List<CellRect> spaces, IntVec3 zExpansion, IntVec3 xExpansion, int xRange, int zRange)
	{
		Rot4 rot = Rot4.West;
		if (rect.Height > rect.Width)
		{
			rot = Rot4.North;
		}
		int offset = (rot.IsHorizontal ? Rand.Range(-zRange, zRange) : Rand.Range(-xRange, xRange));
		IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite, offset);
		IntVec3 centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot, offset);
		CellRect cellRect = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z).ExpandedBy(rot.IsHorizontal ? zExpansion : xExpansion);
		corridors.Add(cellRect);
		DivideRectsByCorridor(rect, cellRect, spaces, rot);
		rot = rot.Rotated(RotationDirection.Clockwise);
		offset = (rot.IsHorizontal ? Rand.Range(-zRange, zRange) : Rand.Range(-xRange, xRange));
		centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite, offset);
		centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot, offset);
		CellRect cellRect2 = ((!rot.IsHorizontal) ? CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, cellRect.minZ) : CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, cellRect.minX, centerCellOnEdge2.z));
		offset = (rot.IsHorizontal ? Rand.Range(-zRange, zRange) : Rand.Range(-xRange, xRange));
		centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite, offset);
		centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot, offset);
		CellRect cellRect3 = ((!rot.IsHorizontal) ? CellRect.FromLimits(centerCellOnEdge.x, cellRect.maxZ, centerCellOnEdge2.x, centerCellOnEdge2.z) : CellRect.FromLimits(cellRect.maxX, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z));
		cellRect2 = cellRect2.ExpandedBy(rot.IsHorizontal ? zExpansion : xExpansion);
		cellRect3 = cellRect3.ExpandedBy(rot.IsHorizontal ? zExpansion : xExpansion);
		corridors.Add(cellRect2);
		corridors.Add(cellRect3);
		DivideRectsByCorridor(spaces[(!spaces[0].Overlaps(cellRect2)) ? 1 : 0], cellRect2, spaces, rot);
		DivideRectsByCorridor(spaces[(!spaces[0].Overlaps(cellRect3)) ? 1 : 0], cellRect3, spaces, rot);
		spaces.RemoveAt(0);
		spaces.RemoveAt(0);
	}

	private static void GenerateTCorridor(CellRect rect, List<CellRect> corridors, List<CellRect> spaces, IntVec3 zExpansion, IntVec3 xExpansion, int xRange, int zRange)
	{
		Rot4 rot = Rot4.East;
		if (rect.Height > rect.Width)
		{
			rot = Rot4.North;
		}
		int offset = ((rot == Rot4.East) ? Rand.Range(-zRange, zRange) : Rand.Range(-xRange, xRange));
		IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite, offset);
		IntVec3 centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot, offset);
		CellRect cellRect = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z).ExpandedBy((rot == Rot4.East) ? zExpansion : xExpansion);
		DivideRectsByCorridor(rect, cellRect, spaces, rot);
		int num = Mathf.RoundToInt(cellRect.CenterCell.DistanceTo(centerCellOnEdge2)) - ((rot == Rot4.East) ? cellRect.Height : cellRect.Width) - ((rot == Rot4.East) ? zRange : xRange);
		if (Rand.Bool)
		{
			rot = rot.Opposite;
		}
		IntVec3 point = cellRect.CenterCell + rot.FacingCell * num;
		rot = rot.Rotated(RotationDirection.Clockwise);
		centerCellOnEdge = rect.GetCellOnEdge(rot.Opposite, point);
		centerCellOnEdge2 = rect.GetCellOnEdge(rot, point);
		CellRect cellRect2 = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z).ExpandedBy(rot.IsHorizontal ? zExpansion : xExpansion);
		if (rot.IsVertical && cellRect2.CenterCell.x >= rect.CenterCell.x)
		{
			cellRect.maxX = cellRect2.minX;
		}
		else if (rot.IsVertical)
		{
			cellRect.minX = cellRect2.maxX;
		}
		else if (cellRect2.CenterCell.z >= rect.CenterCell.z)
		{
			cellRect.maxZ = cellRect2.minZ;
		}
		else
		{
			cellRect.minZ = cellRect2.maxZ;
		}
		corridors.Add(cellRect);
		corridors.Add(cellRect2);
		DivideRectsByCorridor(rect, cellRect2, spaces, rot);
		CellRect value = spaces[0];
		CellRect value2 = spaces[1];
		if (rot.IsVertical && cellRect2.CenterCell.x >= rect.CenterCell.x)
		{
			value.maxX = cellRect2.minX;
			value2.maxX = cellRect2.minX;
			spaces.RemoveAt(2);
		}
		else if (!rot.IsVertical && cellRect2.CenterCell.z >= rect.CenterCell.z)
		{
			value.maxZ = cellRect2.minZ;
			value2.maxZ = cellRect2.minZ;
			spaces.RemoveAt(3);
		}
		else if (rot.IsVertical)
		{
			value.minX = cellRect2.maxX;
			value2.minX = cellRect2.maxX;
			spaces.RemoveAt(2);
		}
		else
		{
			value.minZ = cellRect2.maxZ;
			value2.minZ = cellRect2.maxZ;
			spaces.RemoveAt(3);
		}
		spaces[0] = value;
		spaces[1] = value2;
	}

	private static void GenerateHCorridor(CellRect rect, List<CellRect> corridors, List<CellRect> spaces, IntVec3 zExpansion, IntVec3 xExpansion, int xRange, int zRange)
	{
		Rot4 rot = Rot4.East;
		if (rect.Height > rect.Width)
		{
			rot = Rot4.North;
		}
		int num = ((rot == Rot4.East) ? zRange : xRange);
		IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite, -num);
		IntVec3 centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot, -num);
		CellRect cellRect = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z).ExpandedBy((rot == Rot4.East) ? zExpansion : xExpansion);
		corridors.Add(cellRect);
		DivideRectsByCorridor(rect, cellRect, spaces, rot);
		centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite, num);
		centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot, num);
		CellRect cellRect2 = CellRect.FromLimits(centerCellOnEdge.x, centerCellOnEdge.z, centerCellOnEdge2.x, centerCellOnEdge2.z).ExpandedBy((rot == Rot4.East) ? zExpansion : xExpansion);
		corridors.Add(cellRect2);
		DivideRectsByCorridor(rect, cellRect2, spaces, rot);
		rot = ((rot == Rot4.East) ? Rot4.North : Rot4.East);
		CellRect cellRect3;
		if (rot == Rot4.East)
		{
			int num2 = cellRect.CenterCell.z + Rand.Range(-zRange, zRange);
			cellRect3 = CellRect.FromLimits(cellRect.maxX, num2, cellRect2.minX, num2);
		}
		else
		{
			int num3 = cellRect.CenterCell.x + Rand.Range(-xRange, xRange);
			cellRect3 = CellRect.FromLimits(num3, cellRect.maxZ, num3, cellRect2.minZ);
		}
		cellRect3 = cellRect3.ExpandedBy((rot == Rot4.East) ? zExpansion : xExpansion);
		corridors.Add(cellRect3);
		spaces.RemoveAt(0);
		spaces.RemoveAt(2);
		if (rot == Rot4.East)
		{
			rect.minX = cellRect.maxX;
			rect.maxX = cellRect2.minX;
		}
		else
		{
			rect.minZ = cellRect.maxZ;
			rect.maxZ = cellRect2.minZ;
		}
		DivideRectsByCorridor(rect, cellRect3, spaces, rot);
	}

	private static void DivideRectsByCorridor(CellRect rect, CellRect corridor, List<CellRect> spaces, Rot4 corridorRot)
	{
		CellRect item = rect;
		CellRect item2 = rect;
		if (!rect.Overlaps(corridor))
		{
			Log.Warning($"Tried to divide rects by corridor but rect and corridor are not overlapping. rect: {rect}, corridor: {corridor}, rot: {corridorRot}, \nspaces:\n{spaces.Select((CellRect r) => r.ToString()).ToLineList()}");
		}
		if (corridorRot == Rot4.North)
		{
			item.minX = corridor.maxX;
			item2.maxX = corridor.minX;
		}
		else if (corridorRot == Rot4.East)
		{
			item.minZ = corridor.maxZ;
			item2.maxZ = corridor.minZ;
		}
		else if (corridorRot == Rot4.South)
		{
			item.maxX = corridor.minX;
			item2.minX = corridor.maxX;
		}
		else
		{
			item.maxZ = corridor.minZ;
			item2.minZ = corridor.maxZ;
		}
		spaces.Add(item);
		spaces.Add(item2);
	}

	private static void SplitRandom(CellRect rectToSplit, List<CellRect> rooms, int minWidth, int minHeight)
	{
		rooms.Clear();
		Queue<CellRect> queue = new Queue<CellRect>();
		queue.Enqueue(rectToSplit);
		while (queue.Count > 0)
		{
			CellRect cellRect = queue.Dequeue();
			if (!CanSplit(cellRect))
			{
				rooms.Add(cellRect);
			}
			else if (cellRect.Width > cellRect.Height)
			{
				int num = Rand.Range(minWidth, cellRect.Width - minWidth);
				CellRect item = new CellRect(cellRect.minX, cellRect.minZ, num, cellRect.Height);
				CellRect item2 = new CellRect(cellRect.minX - 1 + num, cellRect.minZ, cellRect.Width - num + 1, cellRect.Height);
				queue.Enqueue(item);
				queue.Enqueue(item2);
			}
			else
			{
				int num2 = Rand.Range(minHeight, cellRect.Height - minHeight);
				CellRect item3 = new CellRect(cellRect.minX, cellRect.minZ - 1 + num2, cellRect.Width, cellRect.Height - num2 + 1);
				CellRect item4 = new CellRect(cellRect.minX, cellRect.minZ, cellRect.Width, num2);
				queue.Enqueue(item3);
				queue.Enqueue(item4);
			}
		}
		bool CanSplit(CellRect r)
		{
			if (r.Height <= 2 * minHeight)
			{
				return r.Width > 2 * minWidth;
			}
			return true;
		}
	}

	private static void MergeRandom(List<CellRect> rects, List<List<CellRect>> mergedRects, IntRange maxMergedRooms, int minAdjacenyScore = 5)
	{
		mergedRects.Clear();
		rects.Shuffle();
		for (int i = 0; i < rects.Count; i++)
		{
			CellRect cellRect = rects[i];
			if (UsedInMerge(mergedRects, cellRect))
			{
				continue;
			}
			List<CellRect> list = new List<CellRect> { cellRect };
			int num = Math.Max(1, maxMergedRooms.RandomInRange);
			for (int j = 0; j < rects.Count; j++)
			{
				if (list.Count >= num)
				{
					break;
				}
				CellRect cellRect2 = rects[j];
				if (!(cellRect == cellRect2) && !UsedInMerge(mergedRects, cellRect2) && cellRect.GetAdjacencyScore(cellRect2) >= minAdjacenyScore)
				{
					list.Add(cellRect2);
				}
			}
			mergedRects.Add(list);
		}
	}

	private static bool UsedInMerge(List<List<CellRect>> mergedRects, CellRect rect)
	{
		for (int i = 0; i < mergedRects.Count; i++)
		{
			for (int j = 0; j < mergedRects[i].Count; j++)
			{
				if (mergedRects[i][j] == rect)
				{
					return true;
				}
			}
		}
		return false;
	}
}
