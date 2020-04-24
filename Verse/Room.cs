using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public sealed class Room
	{
		public sbyte mapIndex = -1;

		private RoomGroup groupInt;

		private List<Region> regions = new List<Region>();

		public int ID = -16161616;

		public int lastChangeTick = -1;

		private int numRegionsTouchingMapEdge;

		private int cachedOpenRoofCount = -1;

		private IEnumerator<IntVec3> cachedOpenRoofState;

		public bool isPrisonCell;

		private int cachedCellCount = -1;

		private bool statsAndRoleDirty = true;

		private DefMap<RoomStatDef, float> stats = new DefMap<RoomStatDef, float>();

		private RoomRoleDef role;

		public int newOrReusedRoomGroupIndex = -1;

		private static int nextRoomID;

		private const int RegionCountHuge = 60;

		private const int MaxRegionsToAssignRoomRole = 36;

		private static readonly Color PrisonFieldColor = new Color(1f, 0.7f, 0.2f);

		private static readonly Color NonPrisonFieldColor = new Color(0.3f, 0.3f, 1f);

		private HashSet<Room> uniqueNeighborsSet = new HashSet<Room>();

		private List<Room> uniqueNeighbors = new List<Room>();

		private HashSet<Thing> uniqueContainedThingsSet = new HashSet<Thing>();

		private List<Thing> uniqueContainedThings = new List<Thing>();

		private HashSet<Thing> uniqueContainedThingsOfDef = new HashSet<Thing>();

		private static List<IntVec3> fields = new List<IntVec3>();

		public Map Map
		{
			get
			{
				if (mapIndex >= 0)
				{
					return Find.Maps[mapIndex];
				}
				return null;
			}
		}

		public RegionType RegionType
		{
			get
			{
				if (!regions.Any())
				{
					return RegionType.None;
				}
				return regions[0].type;
			}
		}

		public List<Region> Regions => regions;

		public int RegionCount => regions.Count;

		public bool IsHuge => regions.Count > 60;

		public bool Dereferenced => regions.Count == 0;

		public bool TouchesMapEdge => numRegionsTouchingMapEdge > 0;

		public float Temperature => Group.Temperature;

		public bool UsesOutdoorTemperature => Group.UsesOutdoorTemperature;

		public RoomGroup Group
		{
			get
			{
				return groupInt;
			}
			set
			{
				if (value != groupInt)
				{
					if (groupInt != null)
					{
						groupInt.RemoveRoom(this);
					}
					groupInt = value;
					if (groupInt != null)
					{
						groupInt.AddRoom(this);
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
					for (int i = 0; i < regions.Count; i++)
					{
						cachedCellCount += regions[i].CellCount;
					}
				}
				return cachedCellCount;
			}
		}

		public int OpenRoofCount => OpenRoofCountStopAt(int.MaxValue);

		public bool PsychologicallyOutdoors
		{
			get
			{
				if (OpenRoofCountStopAt(300) >= 300)
				{
					return true;
				}
				if (Group.AnyRoomTouchesMapEdge && (float)OpenRoofCount / (float)CellCount >= 0.5f)
				{
					return true;
				}
				return false;
			}
		}

		public bool OutdoorsForWork
		{
			get
			{
				if (OpenRoofCountStopAt(101) > 100 || (float)OpenRoofCount > (float)CellCount * 0.25f)
				{
					return true;
				}
				return false;
			}
		}

		public List<Room> Neighbors
		{
			get
			{
				uniqueNeighborsSet.Clear();
				uniqueNeighbors.Clear();
				for (int i = 0; i < regions.Count; i++)
				{
					foreach (Region neighbor in regions[i].Neighbors)
					{
						if (uniqueNeighborsSet.Add(neighbor.Room) && neighbor.Room != this)
						{
							uniqueNeighbors.Add(neighbor.Room);
						}
					}
				}
				uniqueNeighborsSet.Clear();
				return uniqueNeighbors;
			}
		}

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				for (int i = 0; i < regions.Count; i++)
				{
					foreach (IntVec3 cell in regions[i].Cells)
					{
						yield return cell;
					}
				}
			}
		}

		public IEnumerable<IntVec3> BorderCells
		{
			get
			{
				foreach (IntVec3 c in Cells)
				{
					int i = 0;
					while (i < 8)
					{
						IntVec3 intVec = c + GenAdj.AdjacentCells[i];
						Region region = (c + GenAdj.AdjacentCells[i]).GetRegion(Map);
						if (region == null || region.Room != this)
						{
							yield return intVec;
						}
						int num = i + 1;
						i = num;
					}
				}
			}
		}

		public IEnumerable<Pawn> Owners
		{
			get
			{
				if (TouchesMapEdge || IsHuge || (Role != RoomRoleDefOf.Bedroom && Role != RoomRoleDefOf.PrisonCell && Role != RoomRoleDefOf.Barracks && Role != RoomRoleDefOf.PrisonBarracks))
				{
					yield break;
				}
				Pawn pawn = null;
				Pawn secondOwner = null;
				foreach (Building_Bed containedBed in ContainedBeds)
				{
					if (containedBed.def.building.bed_humanlike)
					{
						for (int i = 0; i < containedBed.OwnersForReading.Count; i++)
						{
							if (pawn == null)
							{
								pawn = containedBed.OwnersForReading[i];
							}
							else
							{
								if (secondOwner != null)
								{
									yield break;
								}
								secondOwner = containedBed.OwnersForReading[i];
							}
						}
					}
				}
				if (pawn != null)
				{
					if (secondOwner == null)
					{
						yield return pawn;
					}
					else if (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, secondOwner))
					{
						yield return pawn;
						yield return secondOwner;
					}
				}
			}
		}

		public IEnumerable<Building_Bed> ContainedBeds
		{
			get
			{
				List<Thing> things = ContainedAndAdjacentThings;
				for (int i = 0; i < things.Count; i++)
				{
					Building_Bed building_Bed = things[i] as Building_Bed;
					if (building_Bed != null)
					{
						yield return building_Bed;
					}
				}
			}
		}

		public bool Fogged
		{
			get
			{
				if (regions.Count == 0)
				{
					return false;
				}
				return regions[0].AnyCell.Fogged(Map);
			}
		}

		public bool IsDoorway
		{
			get
			{
				if (regions.Count == 1)
				{
					return regions[0].IsDoorway;
				}
				return false;
			}
		}

		public List<Thing> ContainedAndAdjacentThings
		{
			get
			{
				uniqueContainedThingsSet.Clear();
				uniqueContainedThings.Clear();
				for (int i = 0; i < regions.Count; i++)
				{
					List<Thing> allThings = regions[i].ListerThings.AllThings;
					if (allThings == null)
					{
						continue;
					}
					for (int j = 0; j < allThings.Count; j++)
					{
						Thing item = allThings[j];
						if (uniqueContainedThingsSet.Add(item))
						{
							uniqueContainedThings.Add(item);
						}
					}
				}
				uniqueContainedThingsSet.Clear();
				return uniqueContainedThings;
			}
		}

		public RoomRoleDef Role
		{
			get
			{
				if (statsAndRoleDirty)
				{
					UpdateRoomStatsAndRole();
				}
				return role;
			}
		}

		public static Room MakeNew(Map map)
		{
			Room result = new Room
			{
				mapIndex = (sbyte)map.Index,
				ID = nextRoomID
			};
			nextRoomID++;
			return result;
		}

		public void AddRegion(Region r)
		{
			if (regions.Contains(r))
			{
				Log.Error("Tried to add the same region twice to Room. region=" + r + ", room=" + this);
				return;
			}
			regions.Add(r);
			if (r.touchesMapEdge)
			{
				numRegionsTouchingMapEdge++;
			}
			if (regions.Count == 1)
			{
				Map.regionGrid.allRooms.Add(this);
			}
		}

		public void RemoveRegion(Region r)
		{
			if (!regions.Contains(r))
			{
				Log.Error("Tried to remove region from Room but this region is not here. region=" + r + ", room=" + this);
				return;
			}
			regions.Remove(r);
			if (r.touchesMapEdge)
			{
				numRegionsTouchingMapEdge--;
			}
			if (regions.Count == 0)
			{
				Group = null;
				cachedOpenRoofCount = -1;
				cachedOpenRoofState = null;
				statsAndRoleDirty = true;
				Map.regionGrid.allRooms.Remove(this);
			}
		}

		public void Notify_MyMapRemoved()
		{
			mapIndex = -1;
		}

		public void Notify_ContainedThingSpawnedOrDespawned(Thing th)
		{
			if (th.def.category == ThingCategory.Mote || th.def.category == ThingCategory.Projectile || th.def.category == ThingCategory.Ethereal || th.def.category == ThingCategory.Pawn)
			{
				return;
			}
			if (IsDoorway)
			{
				for (int i = 0; i < regions[0].links.Count; i++)
				{
					Region otherRegion = regions[0].links[i].GetOtherRegion(regions[0]);
					if (!otherRegion.IsDoorway)
					{
						otherRegion.Room.Notify_ContainedThingSpawnedOrDespawned(th);
					}
				}
			}
			statsAndRoleDirty = true;
		}

		public void Notify_TerrainChanged()
		{
			statsAndRoleDirty = true;
		}

		public void Notify_BedTypeChanged()
		{
			statsAndRoleDirty = true;
		}

		public void Notify_RoofChanged()
		{
			cachedOpenRoofCount = -1;
			cachedOpenRoofState = null;
			Group.Notify_RoofChanged();
		}

		public void Notify_RoomShapeOrContainedBedsChanged()
		{
			cachedCellCount = -1;
			cachedOpenRoofCount = -1;
			cachedOpenRoofState = null;
			if (Current.ProgramState == ProgramState.Playing && !Fogged)
			{
				Map.autoBuildRoofAreaSetter.TryGenerateAreaFor(this);
			}
			isPrisonCell = false;
			if (Building_Bed.RoomCanBePrisonCell(this))
			{
				List<Thing> containedAndAdjacentThings = ContainedAndAdjacentThings;
				for (int i = 0; i < containedAndAdjacentThings.Count; i++)
				{
					Building_Bed building_Bed = containedAndAdjacentThings[i] as Building_Bed;
					if (building_Bed != null && building_Bed.ForPrisoners)
					{
						isPrisonCell = true;
						break;
					}
				}
			}
			List<Thing> list = Map.listerThings.ThingsOfDef(ThingDefOf.NutrientPasteDispenser);
			for (int j = 0; j < list.Count; j++)
			{
				list[j].Notify_ColorChanged();
			}
			if (Current.ProgramState == ProgramState.Playing && isPrisonCell)
			{
				foreach (Building_Bed containedBed in ContainedBeds)
				{
					containedBed.ForPrisoners = true;
				}
			}
			lastChangeTick = Find.TickManager.TicksGame;
			statsAndRoleDirty = true;
			FacilitiesUtility.NotifyFacilitiesAboutChangedLOSBlockers(regions);
		}

		public bool ContainsCell(IntVec3 cell)
		{
			if (Map != null)
			{
				return cell.GetRoom(Map, RegionType.Set_All) == this;
			}
			return false;
		}

		public bool ContainsThing(ThingDef def)
		{
			for (int i = 0; i < regions.Count; i++)
			{
				if (regions[i].ListerThings.ThingsOfDef(def).Any())
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerable<Thing> ContainedThings(ThingDef def)
		{
			uniqueContainedThingsOfDef.Clear();
			int j = 0;
			while (j < regions.Count)
			{
				List<Thing> things = regions[j].ListerThings.ThingsOfDef(def);
				int num;
				for (int i = 0; i < things.Count; i = num)
				{
					if (uniqueContainedThingsOfDef.Add(things[i]))
					{
						yield return things[i];
					}
					num = i + 1;
				}
				num = j + 1;
				j = num;
			}
			uniqueContainedThingsOfDef.Clear();
		}

		public int ThingCount(ThingDef def)
		{
			uniqueContainedThingsOfDef.Clear();
			int num = 0;
			for (int i = 0; i < regions.Count; i++)
			{
				List<Thing> list = regions[i].ListerThings.ThingsOfDef(def);
				for (int j = 0; j < list.Count; j++)
				{
					if (uniqueContainedThingsOfDef.Add(list[j]))
					{
						num += list[j].stackCount;
					}
				}
			}
			uniqueContainedThingsOfDef.Clear();
			return num;
		}

		public void DecrementMapIndex()
		{
			if (mapIndex <= 0)
			{
				Log.Warning("Tried to decrement map index for room " + ID + ", but mapIndex=" + mapIndex);
			}
			else
			{
				mapIndex--;
			}
		}

		public float GetStat(RoomStatDef roomStat)
		{
			if (statsAndRoleDirty)
			{
				UpdateRoomStatsAndRole();
			}
			if (stats == null)
			{
				return roomStat.roomlessScore;
			}
			return stats[roomStat];
		}

		public RoomStatScoreStage GetStatScoreStage(RoomStatDef stat)
		{
			return stat.GetScoreStage(GetStat(stat));
		}

		public void DrawFieldEdges()
		{
			fields.Clear();
			fields.AddRange(Cells);
			Color color = isPrisonCell ? PrisonFieldColor : NonPrisonFieldColor;
			color.a = Pulser.PulseBrightness(1f, 0.6f);
			GenDraw.DrawFieldEdges(fields, color);
			fields.Clear();
		}

		public int OpenRoofCountStopAt(int threshold)
		{
			if (cachedOpenRoofCount == -1 && cachedOpenRoofState == null)
			{
				cachedOpenRoofCount = 0;
				cachedOpenRoofState = Cells.GetEnumerator();
			}
			if (cachedOpenRoofCount < threshold && cachedOpenRoofState != null)
			{
				RoofGrid roofGrid = Map.roofGrid;
				while (cachedOpenRoofCount < threshold && cachedOpenRoofState.MoveNext())
				{
					if (!roofGrid.Roofed(cachedOpenRoofState.Current))
					{
						cachedOpenRoofCount++;
					}
				}
				if (cachedOpenRoofCount < threshold)
				{
					cachedOpenRoofState = null;
				}
			}
			return cachedOpenRoofCount;
		}

		private void UpdateRoomStatsAndRole()
		{
			statsAndRoleDirty = false;
			if (!TouchesMapEdge && RegionType == RegionType.Normal && regions.Count <= 36)
			{
				if (stats == null)
				{
					stats = new DefMap<RoomStatDef, float>();
				}
				foreach (RoomStatDef item in DefDatabase<RoomStatDef>.AllDefs.OrderByDescending((RoomStatDef x) => x.updatePriority))
				{
					stats[item] = item.Worker.GetScore(this);
				}
				role = DefDatabase<RoomRoleDef>.AllDefs.MaxBy((RoomRoleDef x) => x.Worker.GetScore(this));
			}
			else
			{
				stats = null;
				role = RoomRoleDefOf.None;
			}
		}

		internal void DebugDraw()
		{
			int hashCode = GetHashCode();
			foreach (IntVec3 cell in Cells)
			{
				CellRenderer.RenderCell(cell, (float)hashCode * 0.01f);
			}
		}

		internal string DebugString()
		{
			return "Room ID=" + ID + "\n  first cell=" + Cells.FirstOrDefault() + "\n  RegionCount=" + RegionCount + "\n  RegionType=" + RegionType + "\n  CellCount=" + CellCount + "\n  OpenRoofCount=" + OpenRoofCount + "\n  numRegionsTouchingMapEdge=" + numRegionsTouchingMapEdge + "\n  lastChangeTick=" + lastChangeTick + "\n  isPrisonCell=" + isPrisonCell.ToString() + "\n  RoomGroup=" + ((Group != null) ? Group.ID.ToString() : "null");
		}

		public override string ToString()
		{
			return "Room(roomID=" + ID + ", first=" + Cells.FirstOrDefault().ToString() + ", RegionsCount=" + RegionCount.ToString() + ", lastChangeTick=" + lastChangeTick + ")";
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineInt(ID, 1538478890);
		}
	}
}
