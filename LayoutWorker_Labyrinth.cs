using System.Collections.Generic;
using DelaunatorSharp;
using RimWorld;
using UnityEngine;
using Verse;

public class LayoutWorker_Labyrinth : LayoutWorker
{
	private static readonly IntRange RoomSizeRange = new IntRange(8, 12);

	private static readonly IntRange LShapeRoomRange = new IntRange(6, 12);

	private static readonly IntRange RoomRange = new IntRange(32, 48);

	private const int CorridorInflation = 3;

	private const int ObeliskRoomSize = 19;

	private static readonly PriorityQueue<IntVec3, int> openSet = new PriorityQueue<IntVec3, int>();

	private static readonly Dictionary<IntVec3, IntVec3> cameFrom = new Dictionary<IntVec3, IntVec3>();

	private static readonly Dictionary<IntVec3, int> gScore = new Dictionary<IntVec3, int>();

	private static readonly Dictionary<IntVec3, int> fScore = new Dictionary<IntVec3, int>();

	private static readonly List<IntVec3> toEnqueue = new List<IntVec3>();

	private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

	public LayoutWorker_Labyrinth(LayoutDef def)
		: base(def)
	{
	}

	protected override LayoutSketch GenerateSketch(StructureGenParams parms)
	{
		if (!ModLister.CheckAnomaly("Labyrinth"))
		{
			return null;
		}
		LayoutSketch layoutSketch = new LayoutSketch
		{
			wall = ThingDefOf.GrayWall,
			door = ThingDefOf.GrayDoor,
			floor = TerrainDefOf.GraySurface,
			defaultAffordanceTerrain = TerrainDefOf.GraySurface,
			wallStuff = ThingDefOf.LabyrinthMatter,
			doorStuff = ThingDefOf.LabyrinthMatter
		};
		using (new ProfilerBlock("Generate Labyrinth"))
		{
			layoutSketch.structureLayout = GenerateLabyrinth(parms);
			return layoutSketch;
		}
	}

	private StructureLayout GenerateLabyrinth(StructureGenParams parms)
	{
		CellRect cellRect = new CellRect(0, 0, parms.size.x, parms.size.z);
		StructureLayout structureLayout = new StructureLayout(parms.sketch, cellRect);
		PlaceObeliskRoom(cellRect, structureLayout);
		using (new ProfilerBlock("Scatter L Rooms"))
		{
			ScatterLRooms(cellRect, structureLayout);
		}
		using (new ProfilerBlock("Scatter Square Rooms"))
		{
			ScatterSquareRooms(cellRect, structureLayout);
		}
		using (new ProfilerBlock("Generate Graphs"))
		{
			GenerateGraphs(structureLayout);
		}
		structureLayout.FinalizeRooms(avoidDoubleWalls: false);
		using (new ProfilerBlock("Create Doors"))
		{
			CreateDoors(structureLayout);
		}
		using (new ProfilerBlock("Create Corridors"))
		{
			CreateCorridorsAStar(structureLayout);
		}
		using (new ProfilerBlock("Fill Empty Spaces"))
		{
			FillEmptySpaces(structureLayout);
			return structureLayout;
		}
	}

	private static void FillEmptySpaces(StructureLayout layout)
	{
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		foreach (IntVec3 cell in layout.container.Cells)
		{
			if (!layout.IsEmptyAt(cell) || hashSet.Contains(cell))
			{
				continue;
			}
			foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(cell))
			{
				if (layout.IsWallAt(item))
				{
					hashSet.Add(cell);
					break;
				}
			}
		}
		foreach (IntVec3 item2 in hashSet)
		{
			layout.Add(item2, RoomLayoutCellType.Wall);
		}
	}

	private static void GenerateGraphs(StructureLayout layout)
	{
		List<Vector2> list = new List<Vector2>();
		foreach (LayoutRoom room in layout.Rooms)
		{
			Vector3 zero = Vector3.zero;
			foreach (CellRect rect in room.rects)
			{
				zero += rect.CenterVector3;
			}
			zero /= (float)room.rects.Count;
			list.Add(new Vector2(zero.x, zero.z));
		}
		layout.delaunator = new Delaunator(list.ToArray());
		layout.neighbours = new RelativeNeighborhoodGraph(layout.delaunator);
	}

	private static void PlaceObeliskRoom(CellRect size, StructureLayout layout)
	{
		int minX = Rand.Range(0, size.Width - 19);
		int minZ = Rand.Range(0, size.Height - 19);
		CellRect item = new CellRect(minX, minZ, 19, 19);
		LayoutRoom layoutRoom = layout.AddRoom(new List<CellRect> { item });
		layoutRoom.requiredDef = LayoutRoomDefOf.LabyrinthObelisk;
		layoutRoom.entryCells = new List<IntVec3>();
		layoutRoom.entryCells.AddRange(item.GetCenterCellsOnEdge(Rot4.North, 2));
		layoutRoom.entryCells.AddRange(item.GetCenterCellsOnEdge(Rot4.East, 2));
		layoutRoom.entryCells.AddRange(item.GetCenterCellsOnEdge(Rot4.South, 2));
		layoutRoom.entryCells.AddRange(item.GetCenterCellsOnEdge(Rot4.West, 2));
	}

	private static void ScatterLRooms(CellRect size, StructureLayout layout)
	{
		int randomInRange = LShapeRoomRange.RandomInRange;
		int num = 0;
		for (int i = 0; i < 100; i++)
		{
			if (num >= randomInRange)
			{
				break;
			}
			int randomInRange2 = RoomSizeRange.RandomInRange;
			int randomInRange3 = RoomSizeRange.RandomInRange;
			int minX = Rand.Range(0, size.Width - randomInRange2);
			int minZ = Rand.Range(0, size.Height - randomInRange3);
			int randomInRange4 = LShapeRoomRange.RandomInRange;
			int randomInRange5 = LShapeRoomRange.RandomInRange;
			while (Mathf.Abs(randomInRange4 - randomInRange2) <= 2)
			{
				randomInRange4 = LShapeRoomRange.RandomInRange;
			}
			while (Mathf.Abs(randomInRange5 - randomInRange3) <= 2)
			{
				randomInRange5 = LShapeRoomRange.RandomInRange;
			}
			CellRect cellRect = new CellRect(minX, minZ, randomInRange2, randomInRange3);
			CellRect cellRect2 = ((!Rand.Bool) ? new CellRect(cellRect.minX - randomInRange4, cellRect.minZ, randomInRange4 + 1, randomInRange5) : new CellRect(cellRect.maxX, cellRect.maxZ - randomInRange5 + 1, randomInRange4, randomInRange5));
			if (cellRect2.Width >= 4 && cellRect2.Height >= 4 && size.FullyContainedWithin(cellRect2) && !OverlapsWithAnyRoom(layout, cellRect) && !OverlapsWithAnyRoom(layout, cellRect2))
			{
				layout.AddRoom(new List<CellRect> { cellRect, cellRect2 });
				num++;
			}
		}
	}

	private static void ScatterSquareRooms(CellRect size, StructureLayout layout)
	{
		int randomInRange = RoomRange.RandomInRange;
		int num = 0;
		for (int i = 0; i < 300; i++)
		{
			if (num >= randomInRange)
			{
				break;
			}
			int randomInRange2 = RoomSizeRange.RandomInRange;
			int randomInRange3 = RoomSizeRange.RandomInRange;
			int minX = Rand.Range(0, size.Width - randomInRange2);
			int minZ = Rand.Range(0, size.Height - randomInRange3);
			CellRect cellRect = new CellRect(minX, minZ, randomInRange2, randomInRange3);
			if (!OverlapsWithAnyRoom(layout, cellRect))
			{
				layout.AddRoom(new List<CellRect> { cellRect });
				num++;
			}
		}
	}

	private static void CreateCorridorsAStar(StructureLayout layout)
	{
		foreach (LayoutRoom room in layout.Rooms)
		{
			foreach (var logicalRoomConnection in layout.GetLogicalRoomConnections(room))
			{
				LayoutRoom item = logicalRoomConnection.Item1;
				if (!room.connections.Contains(item))
				{
					ConnectRooms(layout, room, item);
				}
			}
		}
	}

	private static void ConnectRooms(StructureLayout layout, LayoutRoom a, LayoutRoom b)
	{
		PriorityQueue<(IntVec3, IntVec3), int> priorityQueue = new PriorityQueue<(IntVec3, IntVec3), int>();
		foreach (CellRect rect in a.rects)
		{
			foreach (CellRect rect2 in b.rects)
			{
				IEnumerable<IntVec3> entryCells = a.entryCells;
				foreach (IntVec3 item in entryCells ?? rect.EdgeCells)
				{
					if (rect.IsCorner(item) || rect2.Contains(item))
					{
						continue;
					}
					Rot4 closestEdge = rect.GetClosestEdge(item);
					entryCells = b.entryCells;
					foreach (IntVec3 item2 in entryCells ?? rect2.EdgeCells)
					{
						if (!rect2.IsCorner(item2) && !rect.Contains(item2))
						{
							Rot4 closestEdge2 = rect2.GetClosestEdge(item2);
							int num = (item2 - item).LengthManhattan;
							RotationDirection relativeRotation = Rot4.GetRelativeRotation(closestEdge, closestEdge2);
							if (closestEdge == Rot4.East && item2.x < rect.maxX)
							{
								num += 4;
							}
							else if (closestEdge == Rot4.West && item2.x > rect.minX)
							{
								num += 4;
							}
							if (closestEdge == Rot4.North && item2.z < rect.maxZ)
							{
								num += 4;
							}
							else if (closestEdge == Rot4.South && item2.z > rect.minZ)
							{
								num += 4;
							}
							switch (relativeRotation)
							{
							case RotationDirection.Clockwise:
							case RotationDirection.Counterclockwise:
								num++;
								break;
							case RotationDirection.None:
								num += 2;
								break;
							}
							priorityQueue.Enqueue((item, item2), num);
						}
					}
				}
			}
		}
		(IntVec3, IntVec3) element;
		int priority;
		while (priorityQueue.TryDequeue(out element, out priority))
		{
			var (intVec, intVec2) = element;
			if (!TryGetPath(layout, intVec, intVec2, priority * 2, out var path))
			{
				continue;
			}
			IntVec3 intVec3 = intVec2 - intVec;
			if (Mathf.Max(Mathf.Abs(intVec3.x), Mathf.Abs(intVec3.z)) <= 4)
			{
				layout.Add(intVec, RoomLayoutCellType.Floor);
				layout.Add(intVec2, RoomLayoutCellType.Floor);
				InflatePath(layout, path, 1);
				int index = 1;
				if (path.Count == 1 || !layout.IsGoodForDoor(path[index]))
				{
					index = 0;
				}
				if (a.requiredDef == LayoutRoomDefOf.LabyrinthObelisk)
				{
					layout.Add(intVec, RoomLayoutCellType.Door);
				}
				else if (b.requiredDef == LayoutRoomDefOf.LabyrinthObelisk)
				{
					layout.Add(intVec2, RoomLayoutCellType.Door);
				}
				else
				{
					layout.Add(path[index], RoomLayoutCellType.Door);
				}
			}
			else
			{
				layout.Add(intVec, RoomLayoutCellType.Door);
				layout.Add(intVec2, RoomLayoutCellType.Door);
				InflatePath(layout, path, Mathf.Min(Mathf.Max(1, Mathf.CeilToInt((float)path.Count / 3f)), 3));
			}
			a.connections.Add(b);
			b.connections.Add(a);
			break;
		}
	}

	private static void InflatePath(StructureLayout layout, List<IntVec3> cells, int levels)
	{
		Queue<IntVec3> queue = new Queue<IntVec3>();
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		IntVec3 intVec = cells[0];
		IntVec3 last = cells.GetLast();
		IntVec3 intVec2 = new IntVec3(Mathf.Min(intVec.x, last.x), 0, Mathf.Min(intVec.z, last.z));
		IntVec3 intVec3 = new IntVec3(Mathf.Max(intVec.x, last.x), 0, Mathf.Max(intVec.z, last.z));
		CellRect cellRect = new CellRect
		{
			minX = intVec2.x,
			minZ = intVec2.z,
			maxX = intVec3.x,
			maxZ = intVec3.z
		};
		cellRect = cellRect.ExpandedBy(levels);
		foreach (IntVec3 cell in cells)
		{
			if (layout.IsEmptyAt(cell))
			{
				queue.Enqueue(cell);
				break;
			}
		}
		while (queue.Count != 0)
		{
			IntVec3 intVec4 = queue.Dequeue();
			bool flag = cellRect.IsOnEdge(intVec4) && !cells.Contains(intVec4);
			layout.Add(intVec4, (!flag) ? RoomLayoutCellType.Floor : RoomLayoutCellType.Wall);
			if (flag)
			{
				continue;
			}
			foreach (IntVec3 item in Neighbours8Way(layout, intVec4))
			{
				if (layout.IsEmptyAt(item) && cellRect.Contains(item) && !hashSet.Contains(item))
				{
					queue.Enqueue(item);
					hashSet.Add(item);
				}
			}
		}
		bool flag2;
		do
		{
			flag2 = false;
			foreach (IntVec3 item2 in hashSet)
			{
				if (!layout.IsWallAt(item2) && !cells.Contains(item2) && CountAdjacentWalls(layout, item2) == 3)
				{
					layout.Add(item2, RoomLayoutCellType.Wall);
					flag2 = true;
				}
			}
		}
		while (flag2);
		foreach (IntVec3 edgeCell in cellRect.ExpandedBy(1).EdgeCells)
		{
			if (edgeCell.x > 0 && edgeCell.z > 0 && edgeCell.x < layout.Width && edgeCell.z < layout.Height && !layout.TryGetRoom(edgeCell, out var _) && layout.IsEmptyAt(edgeCell))
			{
				layout.Add(edgeCell, RoomLayoutCellType.Wall);
			}
		}
	}

	private static int CountAdjacentWalls(StructureLayout layout, IntVec3 cell)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			IntVec3 position = cell + new Rot4(i).FacingCell;
			if (position.x > 0 && position.z > 0 && position.x < layout.Width && position.z < layout.Height && layout.IsWallAt(position))
			{
				num++;
			}
		}
		return num;
	}

	private static List<IntVec3> ReconstructPath(Dictionary<IntVec3, IntVec3> from, IntVec3 current)
	{
		List<IntVec3> list = new List<IntVec3> { current };
		while (from.ContainsKey(current))
		{
			current = from[current];
			list.Add(current);
		}
		list.Reverse();
		return list;
	}

	private static void ResetPathVars()
	{
		openSet.Clear();
		cameFrom.Clear();
		gScore.Clear();
		fScore.Clear();
		toEnqueue.Clear();
	}

	private static bool TryGetPath(StructureLayout layout, IntVec3 start, IntVec3 goal, int max, out List<IntVec3> path)
	{
		ResetPathVars();
		gScore.Add(start, 0);
		fScore.Add(start, Heuristic(start, goal));
		openSet.Enqueue(start, fScore[start]);
		while (openSet.Count != 0)
		{
			IntVec3 intVec = openSet.Dequeue();
			if (intVec == goal)
			{
				path = ReconstructPath(cameFrom, intVec);
				ResetPathVars();
				return true;
			}
			toEnqueue.Clear();
			foreach (IntVec3 item in Neighbours(layout, intVec, goal))
			{
				if (item == goal)
				{
					cameFrom[item] = intVec;
					path = ReconstructPath(cameFrom, item);
					ResetPathVars();
					return true;
				}
				int num = gScore[intVec] + 1;
				if (num > max)
				{
					break;
				}
				if (!gScore.ContainsKey(item) || num < gScore[item])
				{
					cameFrom[item] = intVec;
					gScore[item] = num;
					fScore[item] = num + Heuristic(item, goal);
					toEnqueue.Add(item);
				}
			}
			toEnqueue.Sort(delegate(IntVec3 x, IntVec3 z)
			{
				if (x == z)
				{
					return 0;
				}
				IntVec3 intVec2 = x - start;
				if (intVec2.x == 0 || intVec2.z == 0)
				{
					return -1;
				}
				intVec2 = x - goal;
				if (intVec2.x == 0 || intVec2.z == 0)
				{
					return -1;
				}
				intVec2 = z - start;
				if (intVec2.x == 0 || intVec2.z == 0)
				{
					return 1;
				}
				intVec2 = z - goal;
				return (intVec2.x == 0 || intVec2.z == 0) ? 1 : 0;
			});
			foreach (IntVec3 item2 in toEnqueue)
			{
				openSet.Enqueue(item2, fScore[item2]);
			}
		}
		ResetPathVars();
		path = null;
		return false;
	}

	private static IEnumerable<IntVec3> Neighbours8Way(StructureLayout layout, IntVec3 cell)
	{
		IntVec3[] adjacentCellsAround = GenAdj.AdjacentCellsAround;
		foreach (IntVec3 intVec in adjacentCellsAround)
		{
			IntVec3 intVec2 = cell + intVec;
			if (intVec2.x > 0 && intVec2.z > 0 && intVec2.x < layout.Width && intVec2.z < layout.Height && layout.IsEmptyAt(intVec2))
			{
				yield return intVec2;
			}
		}
	}

	private static IEnumerable<IntVec3> Neighbours(StructureLayout layout, IntVec3 cell, IntVec3 goal)
	{
		for (int i = 0; i < 4; i++)
		{
			IntVec3 intVec = cell + new Rot4(i).FacingCell;
			if (intVec.x > 0 && intVec.z > 0 && intVec.x < layout.Width && intVec.z < layout.Height && (!(intVec != goal) || layout.IsEmptyAt(intVec)))
			{
				yield return intVec;
			}
		}
	}

	private static int Heuristic(IntVec3 pos, IntVec3 goal)
	{
		return (goal - pos).LengthManhattan;
	}

	private static bool OverlapsWithAnyRoom(StructureLayout layout, CellRect rect)
	{
		foreach (LayoutRoom room in layout.Rooms)
		{
			foreach (CellRect rect2 in room.rects)
			{
				if (rect2.Overlaps(rect.ContractedBy(1)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void CreateDoors(StructureLayout layout)
	{
		tmpCells.Clear();
		tmpCells.AddRange(layout.container.Cells.InRandomOrder());
		for (int i = 0; i < tmpCells.Count; i++)
		{
			IntVec3 intVec = tmpCells[i];
			if (layout.IsWallAt(intVec))
			{
				if (layout.IsGoodForHorizontalDoor(intVec))
				{
					TryConnectAdjacentRooms(layout, intVec, IntVec3.North);
				}
				if (layout.IsGoodForVerticalDoor(intVec))
				{
					TryConnectAdjacentRooms(layout, intVec, IntVec3.East);
				}
			}
		}
		tmpCells.Clear();
	}

	private static void TryConnectAdjacentRooms(StructureLayout layout, IntVec3 p, IntVec3 dir)
	{
		if (!layout.TryGetRoom(p + dir, out var room) || !layout.TryGetRoom(p - dir, out var room2) || room.connections.Contains(room2))
		{
			return;
		}
		bool flag = false;
		foreach (var logicalRoomConnection in layout.GetLogicalRoomConnections(room))
		{
			if (logicalRoomConnection.Item1 == room2)
			{
				flag = true;
				break;
			}
		}
		if (flag && (room.entryCells == null || room.entryCells.Contains(p)) && (room2.entryCells == null || room2.entryCells.Contains(p)))
		{
			layout.Add(p, RoomLayoutCellType.Door);
			room.connections.Add(room2);
			room2.connections.Add(room);
		}
	}
}
