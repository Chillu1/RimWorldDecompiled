using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StructureLayout : IExposable
{
	public LayoutStructureSketch sketch;

	public CellRect container;

	private List<LayoutRoom> rooms = new List<LayoutRoom>();

	public RoomLayoutCellType[,] cellTypes;

	public int[,] roomIds;

	public Delaunator delaunator;

	public RelativeNeighborhoodGraph neighbours;

	private const int MinAdjacencyForDisconnectedRoom = 3;

	private static readonly List<LayoutRoom> tmpRooms = new List<LayoutRoom>();

	private static readonly Queue<LayoutRoom> tmpRoomQueue = new Queue<LayoutRoom>();

	private static readonly HashSet<LayoutRoom> tmpSeenRooms = new HashSet<LayoutRoom>();

	public List<LayoutRoom> Rooms => rooms;

	public int Width => roomIds.GetLength(0);

	public int Height => roomIds.GetLength(1);

	public int Area
	{
		get
		{
			int num = 0;
			for (int i = 0; i < rooms.Count; i++)
			{
				num += rooms[i].Area;
			}
			return num;
		}
	}

	public StructureLayout(LayoutStructureSketch sketch)
	{
		this.sketch = sketch;
	}

	public StructureLayout(LayoutStructureSketch sketch, CellRect rect)
	{
		this.sketch = sketch;
		container = rect;
		cellTypes = new RoomLayoutCellType[rect.Width, rect.Height];
		roomIds = new int[rect.Width, rect.Height];
		for (int i = 0; i < rect.Width; i++)
		{
			for (int j = 0; j < rect.Height; j++)
			{
				roomIds[i, j] = -1;
			}
		}
	}

	public bool HasRoomWithDef(LayoutRoomDef def)
	{
		return GetFirstRoomOfDef(def) != null;
	}

	public bool TryGetFirstRoomOfDef(LayoutRoomDef def, out LayoutRoom room)
	{
		room = GetFirstRoomOfDef(def);
		return room != null;
	}

	public LayoutRoom GetFirstRoomOfDef(LayoutRoomDef def)
	{
		foreach (LayoutRoom room in rooms)
		{
			if (room.defs != null && room.defs.Contains(def))
			{
				return room;
			}
		}
		return null;
	}

	public LayoutRoom AddRoom(List<CellRect> rects, LayoutRoomDef requiredDef = null)
	{
		for (int i = 0; i < rects.Count; i++)
		{
			rects[i] = rects[i].ClipInsideRect(container);
		}
		LayoutRoom layoutRoom = new LayoutRoom(sketch, rects)
		{
			requiredDef = requiredDef
		};
		rooms.Add(layoutRoom);
		return layoutRoom;
	}

	public void RemoveRoom(LayoutRoom room)
	{
		rooms.Remove(room);
	}

	public void Add(IntVec3 position, RoomLayoutCellType cellType)
	{
		if (cellTypes.InBounds(position.x, position.z))
		{
			cellTypes[position.x, position.z] = cellType;
		}
	}

	public bool IsGoodForHorizontalDoor(IntVec3 p)
	{
		if (IsWallAt(p + IntVec3.West) && IsWallAt(p + IntVec3.East) && !IsWallAt(p + IntVec3.North))
		{
			return !IsWallAt(p + IntVec3.South);
		}
		return false;
	}

	public bool IsGoodForVerticalDoor(IntVec3 p)
	{
		if (IsWallAt(p + IntVec3.North) && IsWallAt(p + IntVec3.South) && !IsWallAt(p + IntVec3.East))
		{
			return !IsWallAt(p + IntVec3.West);
		}
		return false;
	}

	public bool IsGoodForDoor(IntVec3 p)
	{
		if (!IsGoodForHorizontalDoor(p))
		{
			return IsGoodForVerticalDoor(p);
		}
		return true;
	}

	public bool IsWallAt(IntVec3 position)
	{
		if (cellTypes.InBounds(position.x, position.z))
		{
			return cellTypes[position.x, position.z] == RoomLayoutCellType.Wall;
		}
		return false;
	}

	public bool IsFloorAt(IntVec3 position)
	{
		if (cellTypes.InBounds(position.x, position.z))
		{
			return cellTypes[position.x, position.z] == RoomLayoutCellType.Floor;
		}
		return false;
	}

	public bool IsDoorAt(IntVec3 position)
	{
		if (cellTypes.InBounds(position.x, position.z))
		{
			return cellTypes[position.x, position.z] == RoomLayoutCellType.Door;
		}
		return false;
	}

	public bool IsEmptyAt(IntVec3 position)
	{
		if (cellTypes.InBounds(position.x, position.z))
		{
			return cellTypes[position.x, position.z] == RoomLayoutCellType.Empty;
		}
		return false;
	}

	public bool IsOutside(IntVec3 position)
	{
		if (cellTypes.InBounds(position.x, position.z))
		{
			return cellTypes[position.x, position.z] == RoomLayoutCellType.Empty;
		}
		return true;
	}

	public int GetRoomIdAt(IntVec3 position)
	{
		if (!roomIds.InBounds(position.x, position.z))
		{
			return -2;
		}
		return roomIds[position.x, position.z];
	}

	public bool CanRemoveWithoutDisconnection(LayoutRoom room)
	{
		if (WouldDisconnectRoomsIfRemoved(room))
		{
			return false;
		}
		return true;
	}

	public bool TryMinimizeLayoutWithoutDisconnection()
	{
		if (rooms.Count == 1)
		{
			return false;
		}
		for (int num = rooms.Count - 1; num >= 0; num--)
		{
			if (rooms[num].requiredDef == null && !WouldDisconnectRoomsIfRemoved(rooms[num]))
			{
				rooms.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	private bool WouldDisconnectRoomsIfRemoved(LayoutRoom room)
	{
		tmpRooms.Clear();
		tmpRooms.AddRange(rooms);
		tmpRooms.Remove(room);
		tmpSeenRooms.Clear();
		tmpRoomQueue.Clear();
		tmpRoomQueue.Enqueue(tmpRooms.First());
		while (tmpRoomQueue.Count > 0)
		{
			LayoutRoom layoutRoom = tmpRoomQueue.Dequeue();
			tmpSeenRooms.Add(layoutRoom);
			foreach (LayoutRoom tmpRoom in tmpRooms)
			{
				if (layoutRoom != tmpRoom && !tmpSeenRooms.Contains(tmpRoom) && layoutRoom.IsAdjacentTo(tmpRoom, 3))
				{
					tmpRoomQueue.Enqueue(tmpRoom);
				}
			}
		}
		int count = tmpRooms.Count;
		int count2 = tmpSeenRooms.Count;
		tmpRooms.Clear();
		tmpSeenRooms.Clear();
		return count2 != count;
	}

	public bool IsAdjacentToLayoutEdge(LayoutRoom room)
	{
		for (int i = 0; i < room.rects.Count; i++)
		{
			if (room.rects[i].minX == container.minX || room.rects[i].maxX == container.maxX || room.rects[i].minZ == container.minZ || room.rects[i].maxZ == container.maxZ)
			{
				return true;
			}
		}
		return false;
	}

	public void FinalizeRooms(bool avoidDoubleWalls = true)
	{
		for (int i = 0; i < 4; i++)
		{
			Rot4 dir = new Rot4(i);
			foreach (LayoutRoom room in rooms)
			{
				for (int j = 0; j < room.rects.Count; j++)
				{
					foreach (IntVec3 edgeCell in room.rects[j].GetEdgeCells(dir))
					{
						IntVec3 facingCell = dir.FacingCell + edgeCell;
						if (avoidDoubleWalls && (IsWallAt(facingCell) || room.rects.Any((CellRect r) => r.Contains(facingCell))))
						{
							continue;
						}
						bool flag = false;
						foreach (CellRect rect in room.rects)
						{
							if (!(rect == room.rects[j]) && rect.Contains(edgeCell))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							Add(edgeCell, RoomLayoutCellType.Wall);
						}
					}
				}
			}
		}
		for (int num = 0; num < rooms.Count; num++)
		{
			LayoutRoom layoutRoom = rooms[num];
			foreach (CellRect rect2 in layoutRoom.rects)
			{
				foreach (IntVec3 cell in rect2.Cells)
				{
					roomIds[cell.x, cell.z] = num;
					layoutRoom.id = num;
					if (!IsWallAt(cell))
					{
						Add(cell, RoomLayoutCellType.Floor);
					}
				}
			}
		}
		for (int num2 = container.minX; num2 < container.maxX; num2++)
		{
			for (int num3 = container.minZ; num3 < container.maxZ; num3++)
			{
				IntVec3 intVec = new IntVec3(num2, 0, num3);
				if (IsWallAt(intVec) || !IsFloorAt(intVec))
				{
					continue;
				}
				int num4 = 0;
				IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
				foreach (IntVec3 intVec2 in cardinalDirections)
				{
					if (IsWallAt(intVec + intVec2))
					{
						num4++;
					}
				}
				int num6 = 0;
				int num7 = 0;
				cardinalDirections = GenAdj.DiagonalDirections;
				foreach (IntVec3 intVec3 in cardinalDirections)
				{
					if (IsWallAt(intVec + intVec3))
					{
						num6++;
					}
					else if (!IsFloorAt(intVec + intVec3))
					{
						num7++;
					}
				}
				if (num4 > 1 && (num6 < 2 || num7 > 0))
				{
					Add(intVec, RoomLayoutCellType.Wall);
				}
			}
		}
	}

	public bool CellWithinRangeOfUndamageableRoom(IntVec3 cell, float range)
	{
		foreach (LayoutRoom room in Rooms)
		{
			if (!room.DontDestroyWallsDoors)
			{
				continue;
			}
			foreach (CellRect rect in room.rects)
			{
				if (rect.ClosestDistanceTo(cell) <= range)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetLogicalRoomConnectionCount(LayoutRoom room)
	{
		if (neighbours == null || neighbours.connections.NullOrEmpty())
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<Vector2, List<Vector2>> connection in neighbours.connections)
		{
			foreach (CellRect rect in room.rects)
			{
				if (rect.Contains(new IntVec3(Mathf.RoundToInt(connection.Key.x), 0, Mathf.RoundToInt(connection.Key.y))))
				{
					num += connection.Value.Count;
				}
			}
		}
		return num;
	}

	public IEnumerable<(LayoutRoom, CellRect a, CellRect b)> GetLogicalRoomConnections(LayoutRoom room)
	{
		if (neighbours == null || neighbours.connections.NullOrEmpty())
		{
			yield break;
		}
		Dictionary<CellRect, List<Vector2>> connections = new Dictionary<CellRect, List<Vector2>>();
		foreach (KeyValuePair<Vector2, List<Vector2>> connection in neighbours.connections)
		{
			foreach (CellRect rect in room.rects)
			{
				if (rect.Contains(new IntVec3(Mathf.RoundToInt(connection.Key.x), 0, Mathf.RoundToInt(connection.Key.y))))
				{
					connections[rect] = new List<Vector2>();
					connections[rect].AddRange(connection.Value);
				}
			}
		}
		if (connections.Count == 0)
		{
			yield break;
		}
		foreach (LayoutRoom otherRoom in Rooms)
		{
			if (otherRoom == room)
			{
				continue;
			}
			bool found = false;
			foreach (CellRect rect2 in otherRoom.rects)
			{
				foreach (var (item, list2) in connections)
				{
					foreach (Vector2 item2 in list2)
					{
						if (rect2.Contains(new IntVec3(Mathf.RoundToInt(item2.x), 0, Mathf.RoundToInt(item2.y))))
						{
							found = true;
							break;
						}
					}
					if (found)
					{
						yield return (otherRoom, a: item, b: rect2);
						break;
					}
				}
				if (found)
				{
					break;
				}
			}
		}
	}

	public void RemoveLogicalConnectionsBetweenRooms(LayoutRoom a, LayoutRoom b)
	{
		if (neighbours == null || neighbours.connections.NullOrEmpty())
		{
			return;
		}
		foreach (var (point, connections) in neighbours.connections)
		{
			RemoveLinksBetween(a, b, point, connections);
			RemoveLinksBetween(b, a, point, connections);
		}
	}

	private static void RemoveLinksBetween(LayoutRoom a, LayoutRoom b, Vector2 point, List<Vector2> connections)
	{
		IntVec3 c = new IntVec3(Mathf.RoundToInt(point.x), 0, Mathf.RoundToInt(point.y));
		foreach (CellRect rect in a.rects)
		{
			if (!rect.Contains(c))
			{
				continue;
			}
			foreach (Vector2 connection in connections)
			{
				IntVec3 position = new IntVec3(Mathf.RoundToInt(connection.x), 0, Mathf.RoundToInt(connection.y));
				if (b.Contains(position))
				{
					connections.Remove(connection);
					break;
				}
			}
		}
	}

	public bool TryGetRoom(IntVec3 pos, out LayoutRoom room)
	{
		foreach (LayoutRoom room2 in Rooms)
		{
			foreach (CellRect rect in room2.rects)
			{
				if (rect.Contains(pos))
				{
					room = room2;
					return true;
				}
			}
		}
		room = null;
		return false;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref container, "container");
		Scribe_Collections.Look(ref rooms, "rooms", LookMode.Deep);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		for (int num = rooms.Count - 1; num >= 0; num--)
		{
			if (rooms[num] == null)
			{
				rooms.RemoveAt(num);
			}
			else if (rooms[num].sketch == null)
			{
				rooms[num].sketch = sketch;
			}
		}
	}
}
