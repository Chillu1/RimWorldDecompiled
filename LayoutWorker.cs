using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.SketchGen;
using Verse;

public abstract class LayoutWorker
{
	private readonly LayoutDef def;

	private static readonly HashSet<LayoutRoom> tmpMergedRooms = new HashSet<LayoutRoom>();

	protected virtual IntVec3 FlushOffset => IntVec3.Zero;

	public LayoutDef Def => def;

	public LayoutWorker(LayoutDef def)
	{
		this.def = def;
	}

	public LayoutStructureSketch GenerateStructureSketch(StructureGenParams parms)
	{
		parms.sketch = new LayoutStructureSketch
		{
			layoutDef = def,
			uniqueId = $"LayoutStructureSketch_{Find.UniqueIDsManager.GetNextStructureSketchID()}"
		};
		RandBlock randBlock = new RandBlock(MapGenerator.mapBeingGenerated?.NextGenSeed ?? Rand.Int);
		try
		{
			LayoutSketch layoutSketch = GenerateSketch(parms);
			parms.sketch.layoutSketch = layoutSketch;
			parms.sketch.structureLayout = layoutSketch.structureLayout;
			parms.sketch.structureLayout.sketch = parms.sketch;
			ResolveRoomDefs(parms.sketch);
			MergeAdjacentRooms(parms.sketch.structureLayout);
			RemoveBorderDoors(parms.sketch.structureLayout);
			layoutSketch.FlushLayoutToSketch(FlushOffset);
			PostLayoutFlushedToSketch(parms.sketch);
			ResolveRoomSketches(parms.sketch);
			if (def.shouldDamage)
			{
				SketchResolveParams parms2 = new SketchResolveParams
				{
					sketch = parms.sketch.layoutSketch,
					destroyChanceExp = 1.5f
				};
				SketchResolverDefOf.DamageBuildingsLight.Resolve(parms2);
			}
			return parms.sketch;
		}
		finally
		{
			((IDisposable)randBlock/*cast due to .constrained prefix*/).Dispose();
		}
	}

	protected virtual void PostLayoutFlushedToSketch(LayoutStructureSketch parms)
	{
	}

	protected abstract LayoutSketch GenerateSketch(StructureGenParams parms);

	private static void RemoveBorderDoors(StructureLayout layout)
	{
		for (int i = 0; i < layout.Rooms.Count; i++)
		{
			LayoutRoom layoutRoom = layout.Rooms[i];
			for (int j = i + 1; j < layout.Rooms.Count; j++)
			{
				LayoutRoom layoutRoom2 = layout.Rooms[j];
				if (!layoutRoom.connections.Contains(layoutRoom2) || !CanRemoveBorderDoors(layoutRoom, layoutRoom2) || !Rand.Chance(layout.sketch.layoutDef.borderDoorRemoveChance))
				{
					continue;
				}
				foreach (CellRect rect in layoutRoom.rects)
				{
					foreach (IntVec3 edgeCell in rect.EdgeCells)
					{
						if (!layoutRoom2.Contains(edgeCell) || !layout.IsDoorAt(edgeCell))
						{
							continue;
						}
						bool flag = true;
						for (int k = 0; k < 4; k++)
						{
							IntVec3 position = edgeCell + GenAdj.CardinalDirections[k];
							if (layout.IsOutside(position))
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							layout.Add(edgeCell, RoomLayoutCellType.Floor);
						}
					}
				}
			}
		}
	}

	private static void MergeAdjacentRooms(StructureLayout layout)
	{
		for (int i = 0; i < layout.Rooms.Count; i++)
		{
			LayoutRoom layoutRoom = layout.Rooms[i];
			if (tmpMergedRooms.Contains(layoutRoom))
			{
				continue;
			}
			for (int j = i + 1; j < layout.Rooms.Count; j++)
			{
				LayoutRoom layoutRoom2 = layout.Rooms[j];
				if (tmpMergedRooms.Contains(layoutRoom2) || !CanMergeAdjacentRooms(layoutRoom, layoutRoom2) || !layoutRoom.IsAdjacentTo(layoutRoom2, 5) || !Rand.Chance(layout.sketch.layoutDef.adjacentRoomMergeChance))
				{
					continue;
				}
				tmpMergedRooms.Add(layoutRoom);
				tmpMergedRooms.Add(layoutRoom2);
				layoutRoom.merged.Add(layoutRoom2);
				layoutRoom2.merged.Add(layoutRoom);
				foreach (CellRect rect in layoutRoom.rects)
				{
					foreach (IntVec3 edge in rect.EdgeCellsNoCorners)
					{
						bool flag = true;
						for (int k = 0; k < 4; k++)
						{
							IntVec3 position = edge + GenAdj.CardinalDirections[k];
							if (layout.IsOutside(position))
							{
								flag = false;
								break;
							}
						}
						if (flag && layoutRoom2.Contains(edge) && !layoutRoom2.rects.Any((CellRect r) => r.IsCorner(edge)))
						{
							layout.Add(edge, RoomLayoutCellType.Floor);
						}
					}
				}
			}
		}
		tmpMergedRooms.Clear();
	}

	private void ResolveRoomDefs(LayoutStructureSketch sketch)
	{
		if (def.roomDefs.NullOrEmpty())
		{
			return;
		}
		List<LayoutRoomDef> usedDefs = new List<LayoutRoomDef>();
		Dictionary<LayoutRoomDef, int> dictionary = new Dictionary<LayoutRoomDef, int>();
		foreach (LayoutDef.LayoutRoomWeight roomDef2 in def.roomDefs)
		{
			if (roomDef2.countRange.min != 0)
			{
				dictionary.Add(roomDef2.def, roomDef2.countRange.min);
			}
		}
		foreach (LayoutRoom room in sketch.structureLayout.Rooms.InRandomOrder())
		{
			if (room.requiredDef != null)
			{
				room.defs.Add(room.requiredDef);
				usedDefs.Add(room.requiredDef);
				if (dictionary.ContainsKey(room.requiredDef))
				{
					if (dictionary[room.requiredDef] == 1)
					{
						dictionary.Remove(room.requiredDef);
					}
					else
					{
						dictionary[room.requiredDef]--;
					}
				}
				continue;
			}
			if (dictionary.Any())
			{
				bool flag = false;
				foreach (LayoutRoomDef key in dictionary.Keys)
				{
					if (key.CanResolve(room))
					{
						room.defs.Add(key);
						usedDefs.Add(key);
						if (dictionary[key] == 1)
						{
							dictionary.Remove(key);
						}
						else
						{
							dictionary[key]--;
						}
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (def.roomDefs.Where((LayoutDef.LayoutRoomWeight d) => d.def.CanResolve(room) && usedDefs.Count((LayoutRoomDef ud) => ud == d.def) < d.countRange.max).TryRandomElementByWeight((LayoutDef.LayoutRoomWeight d) => d.weight, out var roomDef))
			{
				room.defs.Add(roomDef.def);
				usedDefs.Add(roomDef.def);
				if (def.canHaveMultipleLayoutsInRoom && roomDef.def.canBeInMixedRoom && Rand.Chance(def.multipleLayoutRoomChance) && def.roomDefs.Where((LayoutDef.LayoutRoomWeight d) => d != roomDef && d.def.canBeInMixedRoom && d.def.CanResolve(room) && usedDefs.Count((LayoutRoomDef ud) => ud == d.def) < d.countRange.max).TryRandomElementByWeight((LayoutDef.LayoutRoomWeight d) => d.weight, out var result))
				{
					room.defs.Add(result.def);
					usedDefs.Add(result.def);
				}
			}
		}
		if (dictionary.Any())
		{
			Log.ErrorOnce("Layout failed to spawn all required rooms, rooms failed to place: " + dictionary.Keys.Select((LayoutRoomDef x) => x.defName).ToCommaList(), 114632452);
		}
	}

	private void ResolveRoomSketches(LayoutStructureSketch sketch)
	{
		LayoutRoomParams parms = new LayoutRoomParams
		{
			sketch = sketch.layoutSketch
		};
		foreach (LayoutRoom room in sketch.structureLayout.Rooms)
		{
			if (room.defs.NullOrEmpty())
			{
				continue;
			}
			parms.room = room;
			foreach (LayoutRoomDef def in room.defs)
			{
				room.sketch = sketch;
				def.ResolveSketch(parms);
			}
		}
	}

	public virtual void Spawn(LayoutStructureSketch layoutStructureSketch, Map map, IntVec3 pos, float? threatPoints = null, List<Thing> allSpawnedThings = null, bool roofs = true, bool canReuseSketch = false, Faction faction = null)
	{
		if (layoutStructureSketch.spawned && !canReuseSketch)
		{
			Log.ErrorOnce("Attempted to spawn a structure sketch which was previously spawned, this is not guaranteed to work, confirm you intended to do this by setting the canReuseSketch flag to true.", layoutStructureSketch.GetHashCode());
		}
		IntVec3 offset = ((!layoutStructureSketch.spawned) ? pos : (pos - layoutStructureSketch.center));
		layoutStructureSketch.structureLayout.container = layoutStructureSketch.structureLayout.container.MovedBy(offset);
		ListerBuildings.TrackingScope scope = null;
		if (DebugSettings.logMismatchedLayoutFactions && faction != null)
		{
			scope = map.listerBuildings.Track();
		}
		Thing.allowDestroyNonDestroyable = true;
		LayoutSketch layoutSketch = layoutStructureSketch.layoutSketch;
		bool buildRoofsInstantly = roofs;
		layoutSketch.Spawn(map, pos, faction, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: true, forceTerrainAffordance: true, clearEdificeWhereFloor: true, allSpawnedThings, dormant: false, buildRoofsInstantly, null, null, layoutStructureSketch.layoutSketch?.DefaultAffordanceTerrain);
		Thing.allowDestroyNonDestroyable = false;
		FillAllRooms(layoutStructureSketch, map, pos, layoutStructureSketch.layoutDef.clearRoomsEntirely, faction, threatPoints);
		if (DebugSettings.logMismatchedLayoutFactions && faction != null && scope != null)
		{
			scope.Stop();
			foreach (Building item in from b in map.listerThings.GetThingsOfType<Building>()
				where !scope.Contains(b)
				select b)
			{
				if (item.def.CanHaveFaction && item.Faction != faction)
				{
					Log.Error($"Building {item} at {item.Position} has incorrect faction, faction = {item.Faction?.Name}, should be {faction.Name}");
				}
			}
		}
		scope?.Dispose();
		layoutStructureSketch.spawned = true;
		layoutStructureSketch.center = pos;
	}

	private void FillAllRooms(LayoutStructureSketch structureSketch, Map map, IntVec3 center, bool clearRooms, Faction faction, float? threatPoints = null)
	{
		if (structureSketch.structureLayout.Rooms.NullOrEmpty())
		{
			return;
		}
		IntVec3 intVec = ((!structureSketch.spawned) ? center : (center - structureSketch.center));
		foreach (LayoutRoom room4 in structureSketch.structureLayout.Rooms)
		{
			for (int i = 0; i < room4.rects.Count; i++)
			{
				room4.rects[i] = room4.rects[i].MovedBy(intVec);
			}
		}
		for (int j = 0; j < structureSketch.structureLayout.Rooms.Count; j++)
		{
			LayoutRoom room = structureSketch.structureLayout.Rooms[j];
			using (new RandBlock(HashCode.Combine(structureSketch.id, j)))
			{
				PreFillRoom(map, room, faction, threatPoints);
			}
		}
		for (int k = 0; k < structureSketch.structureLayout.Rooms.Count; k++)
		{
			LayoutRoom room2 = structureSketch.structureLayout.Rooms[k];
			using (new RandBlock(HashCode.Combine(structureSketch.id, k)))
			{
				FillRoom(structureSketch, map, clearRooms, threatPoints, room2, intVec, faction);
			}
		}
		for (int l = 0; l < structureSketch.structureLayout.Rooms.Count; l++)
		{
			LayoutRoom room3 = structureSketch.structureLayout.Rooms[l];
			using (new RandBlock(HashCode.Combine(structureSketch.id, l)))
			{
				PostFillRoom(map, room3, faction, threatPoints);
			}
		}
	}

	protected virtual void PreFillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		if (room.defs.NullOrEmpty())
		{
			return;
		}
		foreach (LayoutRoomDef def in room.defs)
		{
			def.PreResolveContents(map, room, faction, threatPoints);
		}
	}

	protected virtual void PostFillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		if (room.defs.NullOrEmpty())
		{
			return;
		}
		foreach (LayoutRoomDef def in room.defs)
		{
			def.PostResolveContents(map, room, faction, threatPoints);
		}
	}

	protected virtual void FillRoom(LayoutStructureSketch structureSketch, Map map, bool clearRooms, float? threatPoints, LayoutRoom room, IntVec3 delta, Faction faction)
	{
		if (clearRooms)
		{
			ClearThingsInRoom(structureSketch, map, room, delta);
		}
		if (room.defs.NullOrEmpty())
		{
			return;
		}
		map.regionAndRoomUpdater.Enabled = true;
		foreach (LayoutRoomDef def in room.defs)
		{
			def.ResolveContents(map, room, threatPoints, faction);
		}
	}

	private static void ClearThingsInRoom(LayoutStructureSketch structureSketch, Map map, LayoutRoom room, IntVec3 delta)
	{
		foreach (CellRect rect in room.rects)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				structureSketch.layoutSketch.ThingsAt(cell - delta, out var single, out var multiple);
				Building edifice = cell.GetEdifice(map);
				if (edifice != null && ShouldClear(edifice))
				{
					edifice.Destroy();
				}
				foreach (Thing item in cell.GetThingList(map).ToList())
				{
					if (ShouldClear(item))
					{
						item.Destroy();
					}
				}
				bool ShouldClear(Thing thing)
				{
					if (single != null && single.IsSame(thing))
					{
						return false;
					}
					if (multiple != null)
					{
						foreach (SketchThing item2 in multiple)
						{
							if (item2.IsSame(thing))
							{
								return false;
							}
						}
					}
					return true;
				}
			}
		}
	}

	protected static IntVec3 FindBestSpawnLocation(List<LayoutRoom> rooms, ThingDef thingDef, Map map, out LayoutRoom roomUsed, out Rot4 rotUsed, HashSet<LayoutRoom> usedRooms = null)
	{
		foreach (LayoutRoom item in rooms.InRandomOrder())
		{
			if (item.requiredDef != null || (usedRooms != null && usedRooms.Contains(item)))
			{
				continue;
			}
			int num = Rand.Range(0, 3);
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4((i + num) % 4);
				if (ComplexUtility.TryFindRandomSpawnCell(thingDef, item, map, out var spawnPosition, 1, rot))
				{
					roomUsed = item;
					rotUsed = rot;
					return spawnPosition;
				}
			}
		}
		roomUsed = null;
		rotUsed = default(Rot4);
		return IntVec3.Invalid;
	}

	private static bool CanMergeAdjacentRooms(LayoutRoom a, LayoutRoom b)
	{
		foreach (LayoutRoomDef def in a.defs)
		{
			if (!def.canMergeWithAdjacentRoom)
			{
				return false;
			}
		}
		foreach (LayoutRoomDef def2 in b.defs)
		{
			if (!def2.canMergeWithAdjacentRoom)
			{
				return false;
			}
		}
		return true;
	}

	private static bool CanRemoveBorderDoors(LayoutRoom a, LayoutRoom b)
	{
		foreach (LayoutRoomDef def in a.defs)
		{
			if (!def.canRemoveBorderDoors)
			{
				return false;
			}
		}
		foreach (LayoutRoomDef def2 in b.defs)
		{
			if (!def2.canRemoveBorderDoors)
			{
				return false;
			}
		}
		return true;
	}
}
