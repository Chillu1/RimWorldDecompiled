using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public sealed class Region
	{
		public RegionType type = RegionType.Normal;

		public int id = -1;

		public sbyte mapIndex = -1;

		private Room roomInt;

		public List<RegionLink> links = new List<RegionLink>();

		public CellRect extentsClose;

		public CellRect extentsLimit;

		public Building_Door door;

		private int precalculatedHashCode;

		public bool touchesMapEdge;

		private int cachedCellCount = -1;

		public bool valid = true;

		private ListerThings listerThings = new ListerThings(ListerThingsUse.Region);

		public uint[] closedIndex = new uint[RegionTraverser.NumWorkers];

		public uint reachedIndex;

		public int newRegionGroupIndex = -1;

		private Dictionary<Area, AreaOverlap> cachedAreaOverlaps;

		public int mark;

		private List<KeyValuePair<Pawn, Danger>> cachedDangers = new List<KeyValuePair<Pawn, Danger>>();

		private int cachedDangersForFrame;

		private float cachedBaseDesiredPlantsCount;

		private int cachedBaseDesiredPlantsCountForTick = -999999;

		private static Dictionary<Pawn, FloatRange> cachedSafeTemperatureRanges = new Dictionary<Pawn, FloatRange>();

		private static int cachedSafeTemperatureRangesForFrame;

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

		public Room Room
		{
			get
			{
				return roomInt;
			}
			set
			{
				if (value != roomInt)
				{
					if (roomInt != null)
					{
						roomInt.RemoveRegion(this);
					}
					roomInt = value;
					if (roomInt != null)
					{
						roomInt.AddRegion(this);
					}
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
				foreach (RegionLink link in links)
				{
					stringBuilder.AppendLine("  --" + link.ToString());
				}
				stringBuilder.AppendLine("valid: " + valid);
				stringBuilder.AppendLine("makeTick: " + debug_makeTick);
				stringBuilder.AppendLine("roomID: " + ((Room != null) ? Room.ID.ToString() : "null room!"));
				stringBuilder.AppendLine("extentsClose: " + extentsClose);
				stringBuilder.AppendLine("extentsLimit: " + extentsLimit);
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
			if (tp.mode != TraverseMode.PassAllDestroyableThings && tp.mode != TraverseMode.PassAllDestroyableThingsNotWater && !type.Passable())
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
			switch (tp.mode)
			{
			case TraverseMode.ByPawn:
				if (door != null)
				{
					ByteGrid avoidGrid = tp.pawn.GetAvoidGrid();
					if (avoidGrid != null && avoidGrid[door.Position] == byte.MaxValue)
					{
						return false;
					}
					if (tp.pawn.HostileTo(door))
					{
						if (!door.CanPhysicallyPass(tp.pawn))
						{
							return tp.canBash;
						}
						return true;
					}
					if (door.CanPhysicallyPass(tp.pawn))
					{
						return !door.IsForbiddenToPass(tp.pawn);
					}
					return false;
				}
				return true;
			case TraverseMode.NoPassClosedDoors:
				if (door != null)
				{
					return door.FreePassage;
				}
				return true;
			case TraverseMode.PassDoors:
				return true;
			case TraverseMode.PassAllDestroyableThings:
				return true;
			case TraverseMode.NoPassClosedDoorsOrWater:
				if (door != null)
				{
					return door.FreePassage;
				}
				return true;
			case TraverseMode.PassAllDestroyableThingsNotWater:
				return true;
			default:
				throw new NotImplementedException();
			}
		}

		public Danger DangerFor(Pawn p)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (cachedDangersForFrame != Time.frameCount)
				{
					cachedDangers.Clear();
					cachedDangersForFrame = Time.frameCount;
				}
				else
				{
					for (int i = 0; i < cachedDangers.Count; i++)
					{
						if (cachedDangers[i].Key == p)
						{
							return cachedDangers[i].Value;
						}
					}
				}
			}
			float temperature = Room.Temperature;
			FloatRange value;
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (cachedSafeTemperatureRangesForFrame != Time.frameCount)
				{
					cachedSafeTemperatureRanges.Clear();
					cachedSafeTemperatureRangesForFrame = Time.frameCount;
				}
				if (!cachedSafeTemperatureRanges.TryGetValue(p, out value))
				{
					value = p.SafeTemperatureRange();
					cachedSafeTemperatureRanges.Add(p, value);
				}
			}
			else
			{
				value = p.SafeTemperatureRange();
			}
			Danger danger = (value.Includes(temperature) ? Danger.None : ((!value.ExpandedBy(80f).Includes(temperature)) ? Danger.Deadly : Danger.Some));
			if (Current.ProgramState == ProgramState.Playing)
			{
				cachedDangers.Add(new KeyValuePair<Pawn, Danger>(p, danger));
			}
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
			string str = ((door == null) ? "null" : door.ToString());
			return string.Concat("Region(id=", id, ", mapIndex=", mapIndex, ", center=", extentsClose.CenterCell, ", links=", links.Count, ", cells=", CellCount, (door != null) ? (", portal=" + str) : null, ")");
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
				GenDraw.DrawFieldEdges(color: (!valid) ? Color.red : ((!DebugIsNew) ? Color.green : Color.yellow), cells: Cells.ToList());
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
					foreach (IntVec3 cell in link.span.Cells)
					{
						CellRenderer.RenderCell(cell, DebugSolidColorMats.MaterialOf(Color.magenta));
					}
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
			Region region = obj as Region;
			if (region == null)
			{
				return false;
			}
			return region.id == id;
		}
	}
}
