using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class Region
{
	public RegionType type = RegionType.Normal;

	public int id = -1;

	public sbyte mapIndex = -1;

	private District districtInt;

	public List<RegionLink> links = new List<RegionLink>();

	public CellRect extentsClose;

	public CellRect extentsLimit;

	public Building_Door door;

	private int precalculatedHashCode;

	public bool touchesMapEdge;

	private int cachedCellCount = -1;

	public bool valid = true;

	private readonly ListerThings listerThings = new ListerThings(ListerThingsUse.Region);

	public readonly uint[] closedIndex = new uint[RegionTraverser.NumWorkers];

	public uint reachedIndex;

	public int newRegionGroupIndex = -1;

	private Dictionary<Area, AreaOverlap> cachedAreaOverlaps;

	public int mark;

	private readonly Dictionary<Pawn, Danger> cachedDangers = new Dictionary<Pawn, Danger>();

	private int cachedDangersForTick;

	private float cachedBaseDesiredPlantsCount;

	private int cachedBaseDesiredPlantsCountForTick = -999999;

	private static readonly Dictionary<Pawn, FloatRange> cachedSafeTemperatureRanges = new Dictionary<Pawn, FloatRange>();

	private static int cachedSafeTemperatureRangesForTick;

	private int debug_makeTick = -1000;

	private int debug_lastTraverseTick = -1000;

	private static int nextId = 1;

	public const int GridSize = 12;

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

	public IEnumerable<IntVec3> Cells
	{
		get
		{
			RegionGrid regions = Map.regionGrid;
			for (int z = extentsClose.minZ; z <= extentsClose.maxZ; z++)
			{
				for (int x = extentsClose.minX; x <= extentsClose.maxX; x++)
				{
					IntVec3 intVec = new IntVec3(x, 0, z);
					if (regions.GetRegionAt_NoRebuild_InvalidAllowed(intVec) == this)
					{
						yield return intVec;
					}
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
				RegionGrid regionGrid = Map.regionGrid;
				for (int i = extentsClose.minZ; i <= extentsClose.maxZ; i++)
				{
					for (int j = extentsClose.minX; j <= extentsClose.maxX; j++)
					{
						IntVec3 c = new IntVec3(j, 0, i);
						if (regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(c) == this)
						{
							cachedCellCount++;
						}
					}
				}
			}
			return cachedCellCount;
		}
	}

	public IEnumerable<Region> Neighbors
	{
		get
		{
			for (int li = 0; li < links.Count; li++)
			{
				RegionLink link = links[li];
				for (int ri = 0; ri < 2; ri++)
				{
					if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].valid)
					{
						yield return link.regions[ri];
					}
				}
			}
		}
	}

	public IEnumerable<Region> NeighborsOfSameType
	{
		get
		{
			for (int li = 0; li < links.Count; li++)
			{
				RegionLink link = links[li];
				for (int ri = 0; ri < 2; ri++)
				{
					if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].type == type && link.regions[ri].valid)
					{
						yield return link.regions[ri];
					}
				}
			}
		}
	}

	public Room Room => District?.Room;

	public District District
	{
		get
		{
			return districtInt;
		}
		set
		{
			if (value != districtInt)
			{
				districtInt?.RemoveRegion(this);
				districtInt = value;
				districtInt?.AddRegion(this);
			}
		}
	}

	public IntVec3 RandomCell
	{
		get
		{
			Map map = Map;
			CellIndices cellIndices = map.cellIndices;
			Region[] directGrid = map.regionGrid.DirectGrid;
			for (int i = 0; i < 1000; i++)
			{
				IntVec3 randomCell = extentsClose.RandomCell;
				if (directGrid[cellIndices.CellToIndex(randomCell)] == this)
				{
					return randomCell;
				}
			}
			return AnyCell;
		}
	}

	public IntVec3 AnyCell
	{
		get
		{
			Map map = Map;
			CellIndices cellIndices = map.cellIndices;
			Region[] directGrid = map.regionGrid.DirectGrid;
			foreach (IntVec3 item in extentsClose)
			{
				if (directGrid[cellIndices.CellToIndex(item)] == this)
				{
					return item;
				}
			}
			Log.Error("Couldn't find any cell in region " + ToString());
			return extentsClose.RandomCell;
		}
	}

	public string DebugString
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("id: " + id);
			stringBuilder.AppendLine("mapIndex: " + mapIndex);
			stringBuilder.AppendLine("links count: " + links.Count);
			stringBuilder.AppendLine("type: " + type);
			stringBuilder.AppendLine("touchesMapEdge: " + touchesMapEdge);
			foreach (RegionLink link in links)
			{
				stringBuilder.AppendLine("  --" + link);
			}
			stringBuilder.AppendLine("valid: " + valid);
			stringBuilder.AppendLine("makeTick: " + debug_makeTick);
			stringBuilder.AppendLine("districtID: " + ((District != null) ? District.ID.ToString() : "null district!"));
			stringBuilder.AppendLine("roomID: " + ((Room != null) ? Room.ID.ToString() : "null room!"));
			CellRect cellRect = extentsClose;
			stringBuilder.AppendLine("extentsClose: " + cellRect.ToString());
			cellRect = extentsLimit;
			stringBuilder.AppendLine("extentsLimit: " + cellRect.ToString());
			stringBuilder.AppendLine("ListerThings:");
			if (listerThings.AllThings != null)
			{
				for (int i = 0; i < listerThings.AllThings.Count; i++)
				{
					stringBuilder.AppendLine("  --" + listerThings.AllThings[i]);
				}
			}
			return stringBuilder.ToString();
		}
	}

	public bool DebugIsNew => debug_makeTick > Find.TickManager.TicksGame - 60;

	public ListerThings ListerThings => listerThings;

	public bool IsDoorway => door != null;

	private Region()
	{
	}

	public static Region MakeNewUnfilled(IntVec3 root, Map map)
	{
		Region obj = new Region
		{
			debug_makeTick = Find.TickManager.TicksGame,
			id = nextId
		};
		nextId++;
		obj.mapIndex = (sbyte)map.Index;
		obj.precalculatedHashCode = Gen.HashCombineInt(obj.id, 1295813358);
		obj.extentsClose.minX = root.x;
		obj.extentsClose.maxX = root.x;
		obj.extentsClose.minZ = root.z;
		obj.extentsClose.maxZ = root.z;
		obj.extentsLimit.minX = root.x - root.x % 12;
		obj.extentsLimit.maxX = root.x + 12 - (root.x + 12) % 12 - 1;
		obj.extentsLimit.minZ = root.z - root.z % 12;
		obj.extentsLimit.maxZ = root.z + 12 - (root.z + 12) % 12 - 1;
		obj.extentsLimit.ClipInsideMap(map);
		return obj;
	}

	public bool Allows(TraverseParms tp, bool isDestination)
	{
		if (tp.mode != TraverseMode.PassAllDestroyableThings && tp.mode != TraverseMode.PassAllDestroyablePlayerOwnedThings && tp.mode != TraverseMode.PassAllDestroyableThingsNotWater && !type.Passable())
		{
			return false;
		}
		if ((int)tp.maxDanger < 3 && tp.pawn != null)
		{
			Danger danger = DangerFor(tp.pawn);
			if (isDestination || danger == Danger.Deadly)
			{
				Region region = tp.pawn.GetRegion(RegionType.Set_All);
				if ((region == null || (int)danger > (int)region.DangerFor(tp.pawn)) && (int)danger > (int)tp.maxDanger)
				{
					return false;
				}
			}
		}
		bool flag = type == RegionType.Fence && tp.fenceBlocked && !tp.canBashFences;
		switch (tp.mode)
		{
		case TraverseMode.ByPawn:
			if (door != null)
			{
				if (tp.pawn == null)
				{
					if (!door.FreePassage && !door.Open)
					{
						return tp.canBashDoors;
					}
					return true;
				}
				if (tp.pawn.TryGetAvoidGrid(out var grid))
				{
					foreach (IntVec3 item in door.OccupiedRect())
					{
						if (grid.Grid[Map.cellIndices.CellToIndex(item)] == byte.MaxValue && item.GetRegion(Map) == this)
						{
							return false;
						}
					}
				}
				if (tp.pawn.HostileTo(door))
				{
					if (!door.CanPhysicallyPass(tp.pawn))
					{
						return tp.canBashDoors;
					}
					return true;
				}
				if (door.CanPhysicallyPass(tp.pawn))
				{
					return !door.IsForbiddenToPass(tp.pawn);
				}
				return false;
			}
			return !flag;
		case TraverseMode.NoPassClosedDoors:
		case TraverseMode.NoPassClosedDoorsOrWater:
			if (door == null || door.FreePassage)
			{
				return !flag;
			}
			return false;
		case TraverseMode.PassDoors:
			return !flag;
		case TraverseMode.PassAllDestroyableThings:
			return true;
		case TraverseMode.PassAllDestroyablePlayerOwnedThings:
			if (door != null)
			{
				return door.Faction != Faction.OfPlayer;
			}
			return true;
		case TraverseMode.PassAllDestroyableThingsNotWater:
			return true;
		default:
			throw new NotImplementedException();
		}
	}

	public IEnumerable<CellRect> EnumerateRectangleCover()
	{
		List<IntVec3> uncoveredCells = new List<IntVec3>(Cells);
		while (uncoveredCells.Count > 0)
		{
			IntVec3 intVec = uncoveredCells.PopFront();
			int i = intVec.x;
			int num;
			for (num = intVec.z; uncoveredCells.Contains(new IntVec3(i + 1, 0, num)); i++)
			{
			}
			while (true)
			{
				bool flag = false;
				for (int j = intVec.x; j <= i; j++)
				{
					if (!uncoveredCells.Contains(new IntVec3(j, 0, num + 1)))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				num++;
			}
			CellRect cellRect = new CellRect(intVec.x, intVec.z, i - intVec.x + 1, num - intVec.z + 1);
			HashSet<IntVec3> toRemove = new HashSet<IntVec3>(cellRect.Cells);
			uncoveredCells.RemoveAll((IntVec3 x) => toRemove.Contains(x));
			yield return cellRect;
		}
	}

	public Danger DangerFor(Pawn p)
	{
		if (Current.ProgramState == ProgramState.Playing)
		{
			Danger value;
			if (cachedDangersForTick != GenTicks.TicksGame)
			{
				cachedDangers.Clear();
				cachedDangersForTick = GenTicks.TicksGame;
			}
			else if (cachedDangers.TryGetValue(p, out value))
			{
				return value;
			}
		}
		Room room = Room;
		float temperature = room.Temperature;
		FloatRange value2;
		if (Current.ProgramState == ProgramState.Playing)
		{
			if (cachedSafeTemperatureRangesForTick != GenTicks.TicksGame)
			{
				cachedSafeTemperatureRanges.Clear();
				cachedSafeTemperatureRangesForTick = GenTicks.TicksGame;
			}
			if (!cachedSafeTemperatureRanges.TryGetValue(p, out value2))
			{
				value2 = p.SafeTemperatureRange();
				cachedSafeTemperatureRanges.Add(p, value2);
			}
		}
		else
		{
			value2 = p.SafeTemperatureRange();
		}
		Danger danger = (value2.Includes(temperature) ? Danger.None : (value2.ExpandedBy(80f).Includes(temperature) ? Danger.Some : Danger.Deadly));
		if (room.Vacuum > 0.5f && p.ConcernedByVacuum)
		{
			danger = Danger.Deadly;
		}
		cachedDangers[p] = danger;
		return danger;
	}

	public float GetBaseDesiredPlantsCount(bool allowCache = true)
	{
		int ticksGame = Find.TickManager.TicksGame;
		if (allowCache && ticksGame - cachedBaseDesiredPlantsCountForTick < 2500)
		{
			return cachedBaseDesiredPlantsCount;
		}
		cachedBaseDesiredPlantsCount = 0f;
		Map map = Map;
		foreach (IntVec3 cell in Cells)
		{
			cachedBaseDesiredPlantsCount += map.wildPlantSpawner.GetBaseDesiredPlantsCountAt(cell);
		}
		cachedBaseDesiredPlantsCountForTick = ticksGame;
		return cachedBaseDesiredPlantsCount;
	}

	public AreaOverlap OverlapWith(Area a)
	{
		if (a.TrueCount == 0)
		{
			return AreaOverlap.None;
		}
		if (Map != a.Map)
		{
			return AreaOverlap.None;
		}
		if (cachedAreaOverlaps == null)
		{
			cachedAreaOverlaps = new Dictionary<Area, AreaOverlap>();
		}
		if (!cachedAreaOverlaps.TryGetValue(a, out var value))
		{
			int num = 0;
			int num2 = 0;
			foreach (IntVec3 cell in Cells)
			{
				num2++;
				if (a[cell])
				{
					num++;
				}
			}
			value = ((num != 0) ? ((num == num2) ? AreaOverlap.Entire : AreaOverlap.Partial) : AreaOverlap.None);
			cachedAreaOverlaps.Add(a, value);
		}
		return value;
	}

	public void Notify_AreaChanged(Area a)
	{
		if (cachedAreaOverlaps != null && cachedAreaOverlaps.ContainsKey(a))
		{
			cachedAreaOverlaps.Remove(a);
		}
	}

	public void DecrementMapIndex()
	{
		if (mapIndex <= 0)
		{
			Log.Warning("Tried to decrement map index for region " + id + ", but mapIndex=" + mapIndex);
		}
		else
		{
			mapIndex--;
		}
	}

	public void Notify_MyMapRemoved()
	{
		listerThings.Clear();
		mapIndex = -1;
	}

	public static void ClearStaticData()
	{
		cachedSafeTemperatureRanges.Clear();
	}

	public override string ToString()
	{
		string text = ((door == null) ? "null" : door.ToString());
		return "Region(id=" + id + ", mapIndex=" + mapIndex + ", center=" + extentsClose.CenterCell.ToString() + ", links=" + links.Count + ", cells=" + CellCount + ", touchesMapEdge=" + touchesMapEdge + ((door != null) ? (", portal=" + text) : null) + ")";
	}

	public void DebugDraw()
	{
		if (DebugViewSettings.drawRegionTraversal && Find.TickManager.TicksGame < debug_lastTraverseTick + 60)
		{
			float a = 1f - (float)(Find.TickManager.TicksGame - debug_lastTraverseTick) / 60f;
			GenDraw.DrawFieldEdges(Cells.ToList(), new Color(0f, 0f, 1f, a));
		}
	}

	public void DebugDrawMouseover()
	{
		int num = Mathf.RoundToInt(Time.realtimeSinceStartup * 2f) % 2;
		if (DebugViewSettings.drawRegions)
		{
			GenDraw.DrawFieldEdges(Cells.ToList(), DebugColor());
			foreach (Region neighbor in Neighbors)
			{
				GenDraw.DrawFieldEdges(neighbor.Cells.ToList(), Color.grey);
			}
		}
		if (DebugViewSettings.drawRegionLinks)
		{
			foreach (RegionLink link in links)
			{
				if (num != 1)
				{
					continue;
				}
				List<IntVec3> list = link.span.Cells.ToList();
				Material mat = DebugSolidColorMats.MaterialOf(Color.magenta * new Color(1f, 1f, 1f, 0.25f));
				foreach (IntVec3 item in list)
				{
					CellRenderer.RenderCell(item, mat);
				}
				GenDraw.DrawFieldEdges(list, Color.white);
			}
		}
		if (!DebugViewSettings.drawRegionThings)
		{
			return;
		}
		foreach (Thing allThing in listerThings.AllThings)
		{
			CellRenderer.RenderSpot(allThing.TrueCenter(), (float)(allThing.thingIDNumber % 256) / 256f);
		}
	}

	private Color DebugColor()
	{
		if (!valid)
		{
			return Color.red;
		}
		if (DebugIsNew)
		{
			return Color.yellow;
		}
		return Color.green;
	}

	public void Debug_Notify_Traversed()
	{
		debug_lastTraverseTick = Find.TickManager.TicksGame;
	}

	public override int GetHashCode()
	{
		return precalculatedHashCode;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Region region))
		{
			return false;
		}
		return region.id == id;
	}
}
