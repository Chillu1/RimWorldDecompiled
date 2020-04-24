using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class RoomGroup
	{
		public int ID = -1;

		private List<Room> rooms = new List<Room>();

		private RoomGroupTempTracker tempTracker;

		private int cachedOpenRoofCount = -1;

		private int cachedCellCount = -1;

		private static int nextRoomGroupID;

		private const float UseOutdoorTemperatureUnroofedFraction = 0.25f;

		public List<Room> Rooms => rooms;

		public Map Map
		{
			get
			{
				if (!rooms.Any())
				{
					return null;
				}
				return rooms[0].Map;
			}
		}

		public int RoomCount => rooms.Count;

		public RoomGroupTempTracker TempTracker => tempTracker;

		public float Temperature
		{
			get
			{
				return tempTracker.Temperature;
			}
			set
			{
				tempTracker.Temperature = value;
			}
		}

		public bool UsesOutdoorTemperature
		{
			get
			{
				if (!AnyRoomTouchesMapEdge)
				{
					return OpenRoofCount >= Mathf.CeilToInt((float)CellCount * 0.25f);
				}
				return true;
			}
		}

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				for (int i = 0; i < rooms.Count; i++)
				{
					foreach (IntVec3 cell in rooms[i].Cells)
					{
						yield return cell;
					}
				}
			}
		}

		public int CellCount
		{
			get
			{
				if (cachedCellCount == -1)
				{
					cachedCellCount = 0;
					for (int i = 0; i < rooms.Count; i++)
					{
						cachedCellCount += rooms[i].CellCount;
					}
				}
				return cachedCellCount;
			}
		}

		public IEnumerable<Region> Regions
		{
			get
			{
				for (int i = 0; i < rooms.Count; i++)
				{
					foreach (Region region in rooms[i].Regions)
					{
						yield return region;
					}
				}
			}
		}

		public int RegionCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < rooms.Count; i++)
				{
					num += rooms[i].RegionCount;
				}
				return num;
			}
		}

		public int OpenRoofCount
		{
			get
			{
				if (cachedOpenRoofCount == -1)
				{
					cachedOpenRoofCount = 0;
					for (int i = 0; i < rooms.Count; i++)
					{
						cachedOpenRoofCount += rooms[i].OpenRoofCount;
					}
				}
				return cachedOpenRoofCount;
			}
		}

		public bool AnyRoomTouchesMapEdge
		{
			get
			{
				for (int i = 0; i < rooms.Count; i++)
				{
					if (rooms[i].TouchesMapEdge)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static RoomGroup MakeNew(Map map)
		{
			RoomGroup obj = new RoomGroup
			{
				ID = nextRoomGroupID
			};
			obj.tempTracker = new RoomGroupTempTracker(obj, map);
			nextRoomGroupID++;
			return obj;
		}

		public void AddRoom(Room room)
		{
			if (rooms.Contains(room))
			{
				Log.Error("Tried to add the same room twice to RoomGroup. room=" + room + ", roomGroup=" + this);
			}
			else
			{
				rooms.Add(room);
			}
		}

		public void RemoveRoom(Room room)
		{
			if (!rooms.Contains(room))
			{
				Log.Error("Tried to remove room from RoomGroup but this room is not here. room=" + room + ", roomGroup=" + this);
			}
			else
			{
				rooms.Remove(room);
			}
		}

		public bool PushHeat(float energy)
		{
			if (UsesOutdoorTemperature)
			{
				return false;
			}
			Temperature += energy / (float)CellCount;
			return true;
		}

		public void Notify_RoofChanged()
		{
			cachedOpenRoofCount = -1;
			tempTracker.RoofChanged();
		}

		public void Notify_RoomGroupShapeChanged()
		{
			cachedCellCount = -1;
			cachedOpenRoofCount = -1;
			tempTracker.RoomChanged();
		}

		public string DebugString()
		{
			return "RoomGroup ID=" + ID + "\n  first cell=" + Cells.FirstOrDefault() + "\n  RoomCount=" + RoomCount + "\n  RegionCount=" + RegionCount + "\n  CellCount=" + CellCount + "\n  OpenRoofCount=" + OpenRoofCount + "\n  " + tempTracker.DebugString();
		}

		internal void DebugDraw()
		{
			int num = Gen.HashCombineInt(GetHashCode(), 1948571531);
			foreach (IntVec3 cell in Cells)
			{
				CellRenderer.RenderCell(cell, (float)num * 0.01f);
			}
			tempTracker.DebugDraw();
		}
	}
}
