using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Room
{
	public int ID = -1;

	private List<District> districts = new List<District>();

	private RoomTempTracker tempTracker;

	private int cachedOpenRoofCount = -1;

	private int cachedExposedCount = -1;

	private int cachedExposedThreshold;

	private int cachedCellCount = -1;

	private bool isPrisonCell;

	private bool statsAndRoleDirty = true;

	private DefMap<RoomStatDef, float> stats = new DefMap<RoomStatDef, float>();

	private RoomRoleDef role;

	private List<IntVec3> cachedBorderCells = new List<IntVec3>();

	private long cachedBorderCellsTick = -1L;

	private float vacuum;

	public bool vacuumOverlayDrawn;

	public int vacuumOverlayRects;

	private static int nextRoomID;

	private const int RegionCountHuge = 60;

	private const float UseOutdoorTemperatureUnroofedFraction = 0.25f;

	public const float MinVaccum = -0.1f;

	private const int MaxRegionsToAssignRoomRole = 60;

	private static readonly Color PrisonFieldColor = new Color(1f, 0.7f, 0.2f);

	private static readonly Color NonPrisonFieldColor = new Color(0.3f, 0.3f, 1f);

	public const string VacuumChangedSignal = "Vacuum";

	private List<Region> tmpRegions = new List<Region>();

	private readonly HashSet<Thing> uniqueContainedThingsSet = new HashSet<Thing>();

	private readonly List<Thing> uniqueContainedThings = new List<Thing>();

	private readonly HashSet<Thing> uniqueContainedThingsOfDef = new HashSet<Thing>();

	private static List<IntVec3> fields = new List<IntVec3>();

	public List<District> Districts => districts;

	public Map Map
	{
		get
		{
			if (!districts.Any())
			{
				return null;
			}
			return districts[0].Map;
		}
	}

	public int DistrictCount => districts.Count;

	public RoomTempTracker TempTracker => tempTracker;

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
			if (!TouchesMapEdge)
			{
				return OpenRoofCount >= Mathf.CeilToInt((float)CellCount * 0.25f);
			}
			return true;
		}
	}

	public bool ExposedToSpace
	{
		get
		{
			if (Map.Biome.inVacuum)
			{
				if (!TouchesMapEdge)
				{
					return ExposedCountStopAt(1) > 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool Dereferenced => RegionCount == 0;

	public bool IsHuge => RegionCount > 60;

	public bool IsPrisonCell => isPrisonCell;

	public float Vacuum
	{
		get
		{
			if (!Map.Biome.inVacuum)
			{
				return 0f;
			}
			if (ExposedToSpace)
			{
				return vacuum = 1f;
			}
			if (IsDoorway)
			{
				if (!Map.regionAndRoomUpdater.Enabled)
				{
					return 0f;
				}
				float num = 0f;
				Region region = Regions[0];
				foreach (RegionLink link in region.links)
				{
					Room room = link.GetOtherRegion(region)?.Room;
					if (room != null)
					{
						num += room.UnsanitizedVacuum;
					}
				}
				if (Mathf.Approximately(num, 0f) || region.links.Count == 0)
				{
					return 0f;
				}
				num /= (float)region.links.Count;
				return vacuum = ((num <= 0.01f) ? 0f : num);
			}
			if (!(vacuum <= 0.01f))
			{
				return vacuum;
			}
			return 0f;
		}
		set
		{
			vacuum = Mathf.Clamp(value, -0.1f, 1f);
			foreach (Thing uniqueContainedThing in uniqueContainedThings)
			{
				if (uniqueContainedThing is ThingWithComps thingWithComps)
				{
					thingWithComps.BroadcastCompSignal("Vacuum");
				}
			}
		}
	}

	internal float UnsanitizedVacuum => vacuum;

	public IEnumerable<IntVec3> Cells
	{
		get
		{
			for (int i = 0; i < districts.Count; i++)
			{
				foreach (IntVec3 cell in districts[i].Cells)
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
				for (int i = 0; i < districts.Count; i++)
				{
					cachedCellCount += districts[i].CellCount;
				}
			}
			return cachedCellCount;
		}
	}

	public Region FirstRegion
	{
		get
		{
			for (int i = 0; i < districts.Count; i++)
			{
				List<Region> regions = districts[i].Regions;
				if (regions.Count > 0)
				{
					return regions[0];
				}
			}
			return null;
		}
	}

	public List<Region> Regions
	{
		get
		{
			tmpRegions.Clear();
			for (int i = 0; i < districts.Count; i++)
			{
				List<Region> regions = districts[i].Regions;
				for (int j = 0; j < regions.Count; j++)
				{
					tmpRegions.Add(regions[j]);
				}
			}
			return tmpRegions;
		}
	}

	public int RegionCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < districts.Count; i++)
			{
				num += districts[i].RegionCount;
			}
			return num;
		}
	}

	public CellRect ExtentsClose
	{
		get
		{
			CellRect result = new CellRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
			foreach (Region region in Regions)
			{
				if (region.extentsClose.minX < result.minX)
				{
					result.minX = region.extentsClose.minX;
				}
				if (region.extentsClose.minZ < result.minZ)
				{
					result.minZ = region.extentsClose.minZ;
				}
				if (region.extentsClose.maxX > result.maxX)
				{
					result.maxX = region.extentsClose.maxX;
				}
				if (region.extentsClose.maxZ > result.maxZ)
				{
					result.maxZ = region.extentsClose.maxZ;
				}
			}
			return result;
		}
	}

	public int OpenRoofCount
	{
		get
		{
			if (cachedOpenRoofCount == -1)
			{
				cachedOpenRoofCount = OpenRoofCountStopAt(int.MaxValue);
			}
			return cachedOpenRoofCount;
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

	public IEnumerable<IntVec3> BorderCellsCardinal
	{
		get
		{
			foreach (IntVec3 c in Cells)
			{
				int i = 0;
				while (i < 4)
				{
					IntVec3 intVec = c + GenAdj.CardinalDirections[i];
					Region region = intVec.GetRegion(Map);
					if (intVec.InBounds(Map) && (region == null || region.Room != this))
					{
						yield return intVec;
					}
					int num = i + 1;
					i = num;
				}
			}
		}
	}

	public IEnumerable<IntVec3> BorderCellsCached
	{
		get
		{
			if (GenTicks.TicksGame - cachedBorderCellsTick > 120)
			{
				cachedBorderCellsTick = GenTicks.TicksGame;
				cachedBorderCells.Clear();
				cachedBorderCells.AddRange(BorderCells);
			}
			return cachedBorderCells;
		}
	}

	public bool TouchesMapEdge
	{
		get
		{
			for (int i = 0; i < districts.Count; i++)
			{
				if (districts[i].TouchesMapEdge)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool PsychologicallyOutdoors
	{
		get
		{
			if (OpenRoofCountStopAt(300) >= 300)
			{
				return true;
			}
			if (TouchesMapEdge && (float)OpenRoofCount / (float)CellCount >= 0.5f)
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

	public IEnumerable<Pawn> Owners
	{
		get
		{
			if (TouchesMapEdge || IsHuge || (Role != RoomRoleDefOf.Bedroom && Role != RoomRoleDefOf.PrisonCell && Role != RoomRoleDefOf.Barracks && Role != RoomRoleDefOf.PrisonBarracks))
			{
				yield break;
			}
			IEnumerable<Building_Bed> enumerable = ContainedBeds.Where((Building_Bed x) => x.def.building.bed_humanlike);
			if (enumerable.Count() > 1 && (Role == RoomRoleDefOf.Barracks || Role == RoomRoleDefOf.PrisonBarracks) && enumerable.Where((Building_Bed b) => b.OwnersForReading.Any()).Count() > 1)
			{
				yield break;
			}
			foreach (Building_Bed item in enumerable)
			{
				List<Pawn> assignedPawns = item.OwnersForReading;
				for (int i = 0; i < assignedPawns.Count; i++)
				{
					yield return assignedPawns[i];
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
				if (things[i] is Building_Bed building_Bed)
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
			if (RegionCount == 0)
			{
				return false;
			}
			return FirstRegion.AnyCell.Fogged(Map);
		}
	}

	public bool IsDoorway
	{
		get
		{
			if (districts.Count == 1)
			{
				return districts[0].IsDoorway;
			}
			return false;
		}
	}

	public Building_Door Door
	{
		get
		{
			if (!IsDoorway)
			{
				return null;
			}
			return districts[0].Regions[0].door;
		}
	}

	public List<Thing> ContainedAndAdjacentThings
	{
		get
		{
			uniqueContainedThingsSet.Clear();
			uniqueContainedThings.Clear();
			List<Region> regions = Regions;
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

	public bool AnyPassable
	{
		get
		{
			for (int i = 0; i < districts.Count; i++)
			{
				if (districts[i].Passable)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool ProperRoom
	{
		get
		{
			if (TouchesMapEdge)
			{
				return false;
			}
			for (int i = 0; i < districts.Count; i++)
			{
				if (districts[i].RegionType == RegionType.Normal)
				{
					return true;
				}
			}
			return false;
		}
	}

	private int OpenRoofCountStopAt(int threshold)
	{
		if (cachedOpenRoofCount != -1)
		{
			return cachedOpenRoofCount;
		}
		int num = 0;
		for (int i = 0; i < districts.Count; i++)
		{
			num += districts[i].OpenRoofCountStopAt(threshold);
			if (num >= threshold)
			{
				return num;
			}
			threshold -= num;
		}
		return num;
	}

	private int ExposedCountStopAt(int threshold)
	{
		if (cachedExposedThreshold > 0 && cachedExposedThreshold <= threshold)
		{
			return cachedExposedCount;
		}
		cachedExposedThreshold = threshold;
		int num = 0;
		for (int i = 0; i < districts.Count; i++)
		{
			num += districts[i].ExposedVacuumCount(threshold);
			if (num >= threshold)
			{
				return cachedExposedCount = num;
			}
			threshold -= num;
		}
		return cachedExposedCount = num;
	}

	public static Room MakeNew(Map map)
	{
		Room room = new Room();
		room.ID = nextRoomID;
		room.tempTracker = new RoomTempTracker(room, map);
		map.regionAndRoomUpdater.roomLookup[room.ID] = room;
		nextRoomID++;
		return room;
	}

	public void AddDistrict(District district)
	{
		if (districts.Contains(district))
		{
			Log.Error("Tried to add the same district twice to Room. district=" + district?.ToString() + ", room=" + this);
			return;
		}
		districts.Add(district);
		if (districts.Count == 1)
		{
			Map.regionGrid.RoomAdded(this);
		}
	}

	public void RemoveDistrict(District district)
	{
		if (!districts.Contains(district))
		{
			Log.Error("Tried to remove district from Room but this district is not here. district=" + district?.ToString() + ", room=" + this);
			return;
		}
		Map map = Map;
		districts.Remove(district);
		tmpRegions.Clear();
		if (districts.Count == 0)
		{
			map?.regionGrid.RoomRemoved(this);
		}
		statsAndRoleDirty = true;
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

	public void Notify_ContainedThingSpawnedOrDespawned(Thing th)
	{
		if (th.def.category == ThingCategory.Mote || th.def.category == ThingCategory.Projectile || th.def.category == ThingCategory.Ethereal || th.def.category == ThingCategory.Pawn)
		{
			return;
		}
		if (IsDoorway)
		{
			List<Region> regions = districts[0].Regions;
			for (int i = 0; i < regions[0].links.Count; i++)
			{
				Region otherRegion = regions[0].links[i].GetOtherRegion(regions[0]);
				if (otherRegion != null && !otherRegion.IsDoorway)
				{
					otherRegion.Room.Notify_ContainedThingSpawnedOrDespawned(th);
				}
			}
		}
		statsAndRoleDirty = true;
	}

	public void Notify_TerrainChanged()
	{
		cachedExposedCount = -1;
		cachedExposedThreshold = 0;
		statsAndRoleDirty = true;
	}

	public void Notify_BedTypeChanged()
	{
		statsAndRoleDirty = true;
	}

	public void Notify_RoofChanged()
	{
		cachedOpenRoofCount = -1;
		cachedExposedCount = -1;
		cachedExposedThreshold = 0;
		tempTracker.RoofChanged();
	}

	public void Notify_RoomShapeChanged()
	{
		cachedCellCount = -1;
		cachedOpenRoofCount = -1;
		cachedExposedCount = -1;
		cachedExposedThreshold = 0;
		if (Dereferenced)
		{
			isPrisonCell = false;
			statsAndRoleDirty = true;
			return;
		}
		tempTracker.RoomChanged();
		if (Current.ProgramState == ProgramState.Playing && !Fogged && !GravshipPlacementUtility.placingGravship)
		{
			Map.autoBuildRoofAreaSetter.TryGenerateAreaFor(this);
		}
		isPrisonCell = false;
		if (Building_Bed.RoomCanBePrisonCell(this))
		{
			List<Thing> containedAndAdjacentThings = ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				if (containedAndAdjacentThings[i] is Building_Bed { ForPrisoners: not false })
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
		statsAndRoleDirty = true;
	}

	public bool ContainsCell(IntVec3 cell)
	{
		if (Map != null)
		{
			return cell.GetRoom(Map) == this;
		}
		return false;
	}

	public bool ContainsThing(ThingDef def)
	{
		List<Region> regions = Regions;
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
		List<Region> regions = Regions;
		int i = 0;
		while (i < regions.Count)
		{
			List<Thing> things = regions[i].ListerThings.ThingsOfDef(def);
			int num;
			for (int j = 0; j < things.Count; j = num)
			{
				if (uniqueContainedThingsOfDef.Add(things[j]))
				{
					yield return things[j];
				}
				num = j + 1;
			}
			num = i + 1;
			i = num;
		}
		uniqueContainedThingsOfDef.Clear();
	}

	public IEnumerable<T> ContainedThings<T>() where T : Thing
	{
		uniqueContainedThingsOfDef.Clear();
		foreach (Region region in Regions)
		{
			foreach (T item in region.ListerThings.GetThingsOfType<T>())
			{
				if (uniqueContainedThingsOfDef.Add(item))
				{
					yield return item;
				}
			}
		}
		uniqueContainedThingsOfDef.Clear();
	}

	public IEnumerable<Thing> ContainedThingsList(IEnumerable<ThingDef> thingDefs)
	{
		foreach (ThingDef thingDef in thingDefs)
		{
			foreach (Thing item in ContainedThings(thingDef))
			{
				yield return item;
			}
		}
	}

	public int ThingCount(ThingDef def)
	{
		uniqueContainedThingsOfDef.Clear();
		List<Region> regions = Regions;
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

	public void DrawFieldEdges()
	{
		fields.Clear();
		fields.AddRange(Cells);
		Color color = (isPrisonCell ? PrisonFieldColor : NonPrisonFieldColor);
		color.a = Pulser.PulseBrightness(1f, 0.6f);
		GenDraw.DrawFieldEdges(fields, color);
		fields.Clear();
	}

	public RoomRoleDef GetRoomRoleIfBuildingPlaced(ThingDef buildingDef)
	{
		if (statsAndRoleDirty)
		{
			UpdateRoomStatsAndRole();
		}
		if (!ProperRoom || RegionCount > 60)
		{
			return RoomRoleDefOf.None;
		}
		return DefDatabase<RoomRoleDef>.AllDefs.MaxBy((RoomRoleDef x) => x.Worker.GetScore(this) + x.Worker.GetScoreDeltaIfBuildingPlaced(this, buildingDef));
	}

	private void UpdateRoomStatsAndRole()
	{
		statsAndRoleDirty = false;
		if (ProperRoom && RegionCount <= 60)
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

	public string GetRoomRoleLabel()
	{
		Pawn pawn = null;
		Pawn pawn2 = null;
		foreach (Pawn owner in Owners)
		{
			if (pawn == null)
			{
				pawn = owner;
			}
			else
			{
				pawn2 = owner;
			}
		}
		TaggedString taggedString = ((pawn == null) ? ((TaggedString)Role.PostProcessedLabel(this)) : ((pawn2 != null && pawn2 != pawn) ? "CouplesRoom".Translate(pawn.LabelShort, pawn2.LabelShort, Role.label, pawn.Named("PAWN1"), pawn2.Named("PAWN2")) : "SomeonesRoom".Translate(pawn.LabelShort, Role.label, pawn.Named("PAWN"))));
		return taggedString;
	}

	public string DebugString()
	{
		return "Room ID=" + ID + "\n  first cell=" + Cells.FirstOrDefault().ToString() + "\n  DistrictCount=" + DistrictCount + "\n  RegionCount=" + RegionCount + "\n  CellCount=" + CellCount + "\n  OpenRoofCount=" + OpenRoofCount + "\n  PsychologicallyOutdoors=" + PsychologicallyOutdoors + "\n  OutdoorsForWork=" + OutdoorsForWork + "\n  WellEnclosed=" + ProperRoom + "\n  " + tempTracker.DebugString() + (DebugViewSettings.writeRoomRoles ? ("\n" + DebugRolesString()) : "");
	}

	private string DebugRolesString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (var (num, arg) in from x in DefDatabase<RoomRoleDef>.AllDefs
			select (score: x.Worker.GetScore(this), role: x) into tuple2
			orderby tuple2.score descending
			select tuple2)
		{
			stringBuilder.AppendLine($"{num}: {arg}");
		}
		return stringBuilder.ToString();
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

	public override string ToString()
	{
		return "Room(roomID=" + ID + ", first=" + Cells.FirstOrDefault().ToString() + ", RegionsCount=" + RegionCount + ")";
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineInt(ID, 1538478891);
	}
}
