using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

public abstract class LayoutWorker_Structure : LayoutWorker
{
	private static readonly List<CellRect> TmpSubdivided = new List<CellRect>(6);

	private static readonly List<IntVec3> TmpCells = new List<IntVec3>();

	private static readonly List<IntVec3> UsedSurroundingCells = new List<IntVec3>(200);

	public new StructureLayoutDef Def => (StructureLayoutDef)base.Def;

	protected virtual float RoomToExteriorDoorRatio => Def.roomToExteriorDoorRatio;

	protected virtual bool CanConnectRoomsExternally => Def.canConnectRoomsExternally;

	protected virtual TerrainDef Terrain => Def.terrainDef;

	protected virtual ThingDef WallStuff => Def.wallStuffDef;

	protected virtual ThingDef DoorStuff => Def.doorStuffDef;

	protected virtual ThingDef WallThing => Def.wallDef;

	protected virtual ThingDef DoorThing => Def.doorDef;

	protected virtual ThingDef WallLampThing => Def.wallLampDef;

	protected virtual TerrainDef DefaultAffordanceTerrain => Def.defaultAffordanceTerrainDef;

	protected virtual ThingDef ForceExteriorDoor => Def.exteriorDoorDef;

	protected virtual ThingDef ForceExteriorDoorStuff => Def.exteriorDoorStuffDef;

	protected virtual bool SpawnDoors => Def.spawnDoors;

	protected virtual LayoutRoomDef ImportantRoomDef => Def.importantRoomDef;

	protected virtual TerrainDef SurroundingTerrain => Def.surroundingTerrainDef;

	protected virtual TerrainDef SurroundingScatterTerrain => Def.surroundingScatterTerrainDef;

	protected virtual ThingDef GetWallDoorStuff(StructureGenParams parms)
	{
		return BaseGenUtility.RandomCheapWallStuff(parms.faction ?? Faction.OfAncients, notVeryFlammable: true);
	}

	public LayoutWorker_Structure(LayoutDef def)
		: base(def)
	{
	}

	protected override LayoutSketch GenerateSketch(StructureGenParams parms)
	{
		ThingDef wallDoorStuff = GetWallDoorStuff(parms);
		ThingDef thingDef = WallThing ?? ThingDefOf.Wall;
		ThingDef thingDef2 = DoorThing ?? ThingDefOf.Door;
		LayoutSketch layoutSketch = new LayoutSketch
		{
			floor = (Terrain ?? TerrainDefOf.AncientConcrete),
			wall = thingDef,
			door = thingDef2,
			wallStuff = (WallStuff ?? ((wallDoorStuff == null || !thingDef.MadeFromStuff) ? null : wallDoorStuff)),
			doorStuff = (DoorStuff ?? ((wallDoorStuff == null || !thingDef2.MadeFromStuff) ? null : wallDoorStuff)),
			wallLamp = WallLampThing,
			defaultAffordanceTerrain = DefaultAffordanceTerrain
		};
		using (new ProfilerBlock("GenerateStructure()"))
		{
			layoutSketch.structureLayout = GenerateStructure(parms);
			return layoutSketch;
		}
	}

	protected virtual StructureLayout GenerateStructure(StructureGenParams parms)
	{
		CellRect rect = new CellRect(0, 0, parms.size.x, parms.size.z);
		StructureLayout structureLayout = GetStructureLayout(parms, rect);
		structureLayout.sketch = parms.sketch;
		using (new ProfilerBlock("Generate Graphs"))
		{
			GenerateGraphs(structureLayout);
		}
		PostGraphsGenerated(structureLayout, parms);
		if (SpawnDoors)
		{
			CreateDoors(structureLayout);
		}
		return structureLayout;
	}

	protected virtual void PostGraphsGenerated(StructureLayout layout, StructureGenParams parms)
	{
		LayoutRoomDef importantRoomDef = ImportantRoomDef;
		if (importantRoomDef == null || !parms.spawnImportantRoom)
		{
			return;
		}
		CellRect rect;
		foreach (LayoutRoom item2 in from r in layout.Rooms
			where r.requiredDef == null && r.TryGetRectOfSize(7, 7, out rect)
			orderby r.Area descending
			select r)
		{
			foreach (var logicalRoomConnection in layout.GetLogicalRoomConnections(item2))
			{
				LayoutRoom item = logicalRoomConnection.Item1;
				if (item2.IsAdjacentTo(item, 2))
				{
					item2.requiredDef = importantRoomDef;
					item2.noExteriorDoors = true;
					return;
				}
			}
		}
		LayoutRoom layoutRoom = layout.Rooms.OrderByDescending((LayoutRoom r) => r.Area).First();
		layoutRoom.requiredDef = importantRoomDef;
		layoutRoom.noExteriorDoors = true;
	}

	protected abstract StructureLayout GetStructureLayout(StructureGenParams parms, CellRect rect);

	public override void Spawn(LayoutStructureSketch layoutStructureSketch, Map map, IntVec3 pos, float? threatPoints = null, List<Thing> allSpawnedThings = null, bool roofs = true, bool canReuseSketch = false, Faction faction = null)
	{
		bool num = Def.wallDamageRange != IntRange.Invalid || Def.clearDoorFaction || Def.ensureOneDoorUnlocked;
		ListerBuildings.TrackingScope trackingScope = null;
		if (num)
		{
			trackingScope = map.listerBuildings.Track();
		}
		base.Spawn(layoutStructureSketch, map, pos, threatPoints, allSpawnedThings, roofs: false, canReuseSketch, faction);
		if (num)
		{
			trackingScope.Stop();
			bool flag = false;
			foreach (Building item in trackingScope)
			{
				if (item.def.IsWall && Def.wallDamageRange != IntRange.Invalid)
				{
					item.HitPoints -= Def.wallDamageRange.RandomInRange;
				}
				if (item is Building_Door && Def.clearDoorFaction)
				{
					item.SetFaction(null);
				}
				if (Def.ensureOneDoorUnlocked && !flag && item is Building_HackableDoor building_HackableDoor && IsOnExterior(layoutStructureSketch.structureLayout, building_HackableDoor.Position))
				{
					building_HackableDoor.Hackable.HackNow();
					flag = true;
				}
			}
		}
		trackingScope?.Dispose();
		PlaceSurroundingTerrain(layoutStructureSketch, map, faction);
		ScatterPrefabs(map, layoutStructureSketch.structureLayout.container);
	}

	private void ScatterPrefabs(Map map, CellRect structure)
	{
		MapGenUtility.SpawnScatteredGroupPrefabs(map, structure, Def.scatteredPrefabs);
	}

	protected override void PostLayoutFlushedToSketch(LayoutStructureSketch parms)
	{
		ReplaceExteriorDoors(parms.layoutSketch);
	}

	private static void GenerateGraphs(StructureLayout layout)
	{
		List<Vector2> list = new List<Vector2>();
		foreach (LayoutRoom room in layout.Rooms)
		{
			foreach (CellRect rect in room.rects)
			{
				if (rect.Width < 25 && rect.Height < 25)
				{
					Vector3 centerVector = rect.CenterVector3;
					list.Add(new Vector2(centerVector.x, centerVector.z));
					continue;
				}
				rect.SubdivideToMaxLength(25, TmpSubdivided);
				foreach (CellRect item in TmpSubdivided)
				{
					Vector3 centerVector2 = item.CenterVector3;
					list.Add(new Vector2(centerVector2.x, centerVector2.z));
				}
			}
		}
		if (list.Count >= 3)
		{
			layout.delaunator = new Delaunator(list.ToArray());
			layout.neighbours = new RelativeNeighborhoodGraph(layout.delaunator);
		}
	}

	private void CreateDoors(StructureLayout layout)
	{
		foreach (LayoutRoom room in layout.Rooms)
		{
			foreach (var logicalRoomConnection in layout.GetLogicalRoomConnections(room))
			{
				var (layoutRoom, aRect, bRect) = logicalRoomConnection;
				if (!room.connections.Contains(layoutRoom) && (TryGetBestDoorCell(layout, aRect, bRect, out var cell, IsCenterAlignedCell) || TryGetBestDoorCell(layout, aRect, bRect, out cell, NotNearCorner) || TryGetBestDoorCell(layout, aRect, bRect, out cell)))
				{
					layout.Add(cell, RoomLayoutCellType.Door);
					MarkConnected(room, layoutRoom);
				}
				bool NotNearCorner(IntVec3 bCenter, IntVec3 pos)
				{
					return aRect.ClosestCorner(pos).DistanceTo(pos) > 1f;
				}
			}
		}
		int exteriorDoors = 0;
		if (CanConnectRoomsExternally)
		{
			ConnectViaExteriorDoors(layout, ref exteriorDoors);
		}
		PlaceExtraExteriorDoors(layout, ref exteriorDoors);
		static bool IsCenterAlignedCell(IntVec3 bCenter, IntVec3 pos)
		{
			if (pos.x != bCenter.x)
			{
				return pos.z == bCenter.z;
			}
			return true;
		}
	}

	private static void ConnectViaExteriorDoors(StructureLayout layout, ref int exteriorDoors)
	{
		foreach (LayoutRoom room in layout.Rooms)
		{
			if (room.noExteriorDoors)
			{
				continue;
			}
			foreach (var (layoutRoom, aRect, bRect) in layout.GetLogicalRoomConnections(room))
			{
				if (room.connections.Contains(layoutRoom) || layoutRoom.noExteriorDoors)
				{
					continue;
				}
				var (intVec, position) = GetBestExteriorDoorCells(layout, aRect, bRect);
				if (intVec.IsValid)
				{
					layout.Add(intVec, RoomLayoutCellType.Door);
					IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
					foreach (IntVec3 intVec2 in cardinalDirectionsAround)
					{
						IntVec3 c = intVec + intVec2;
						if (!layout.container.Contains(c))
						{
							exteriorDoors++;
						}
					}
				}
				if (position.IsValid)
				{
					layout.Add(position, RoomLayoutCellType.Door);
					IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
					foreach (IntVec3 intVec3 in cardinalDirectionsAround)
					{
						IntVec3 c2 = intVec + intVec3;
						if (!layout.container.Contains(c2))
						{
							exteriorDoors++;
						}
					}
				}
				MarkConnected(room, layoutRoom);
			}
		}
	}

	private void PlaceExtraExteriorDoors(StructureLayout layout, ref int exteriorDoors)
	{
		if (RoomToExteriorDoorRatio <= 0f)
		{
			return;
		}
		int num = Mathf.Max(Mathf.RoundToInt((float)layout.Rooms.Count * RoomToExteriorDoorRatio), 2);
		int num2 = 100;
		while (exteriorDoors < num && num2-- > 0)
		{
			foreach (LayoutRoom room in layout.Rooms)
			{
				if (room.noExteriorDoors)
				{
					continue;
				}
				foreach (CellRect rect in room.rects)
				{
					IntVec3 targetCenter = rect.CenterCell + GenAdj.CardinalDirections[Rand.Range(0, 4)] * 50;
					if (TryGetBestRectExteriorCell(layout, rect, targetCenter, out var cell, desperate: false))
					{
						layout.Add(cell, RoomLayoutCellType.Door);
						exteriorDoors++;
						break;
					}
				}
				if (exteriorDoors >= num)
				{
					break;
				}
			}
		}
	}

	private static bool TryGetBestDoorCell(StructureLayout layout, CellRect aRect, CellRect bRect, out IntVec3 cell, Predicate<IntVec3, IntVec3> validator = null)
	{
		float num = float.MaxValue;
		cell = IntVec3.Invalid;
		if (!aRect.ExpandedBy(1).Overlaps(bRect))
		{
			return false;
		}
		IntVec3 centerCell = bRect.CenterCell;
		foreach (IntVec3 edgeCell in aRect.EdgeCells)
		{
			if ((validator == null || validator(centerCell, edgeCell)) && !aRect.IsCorner(edgeCell) && IsGoodForDoor(layout, aRect, bRect, edgeCell))
			{
				float num2 = centerCell.DistanceTo(edgeCell);
				if (num2 < num)
				{
					num = num2;
					cell = edgeCell;
				}
			}
		}
		return cell.IsValid;
	}

	private static bool IsGoodForDoor(StructureLayout layout, CellRect aRect, CellRect bRect, IntVec3 pos)
	{
		if (!IsGoodForHorizontalDoor(layout, aRect, bRect, pos))
		{
			return IsGoodForVerticalDoor(layout, aRect, bRect, pos);
		}
		return true;
	}

	private static bool IsGoodForHorizontalDoor(StructureLayout layout, CellRect aRect, CellRect bRect, IntVec3 pos)
	{
		if (!layout.IsGoodForHorizontalDoor(pos))
		{
			return false;
		}
		if (bRect.Contains(pos + IntVec3.South) || bRect.Contains(pos + IntVec3.North))
		{
			if (!aRect.Contains(pos + IntVec3.South))
			{
				return aRect.Contains(pos + IntVec3.North);
			}
			return true;
		}
		return false;
	}

	private static bool IsGoodForVerticalDoor(StructureLayout layout, CellRect aRect, CellRect bRect, IntVec3 pos)
	{
		if (!layout.IsGoodForVerticalDoor(pos))
		{
			return false;
		}
		if (bRect.Contains(pos + IntVec3.East) || bRect.Contains(pos + IntVec3.West))
		{
			if (!aRect.Contains(pos + IntVec3.East))
			{
				return aRect.Contains(pos + IntVec3.West);
			}
			return true;
		}
		return false;
	}

	private static (IntVec3 a, IntVec3 b) GetBestExteriorDoorCells(StructureLayout layout, CellRect aRect, CellRect bRect)
	{
		TryGetBestRectExteriorCell(layout, aRect, bRect.CenterCell, out var cell, desperate: true);
		TryGetBestRectExteriorCell(layout, bRect, aRect.CenterCell, out var cell2, desperate: true);
		return (a: cell, b: cell2);
	}

	private static bool TryGetBestRectExteriorCell(StructureLayout layout, CellRect rect, IntVec3 targetCenter, out IntVec3 cell, bool desperate)
	{
		foreach (IntVec3 edgeCell in rect.EdgeCells)
		{
			if (!layout.IsGoodForDoor(edgeCell))
			{
				continue;
			}
			bool flag = false;
			IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
			foreach (IntVec3 intVec in cardinalDirectionsAround)
			{
				if (!AnyRoomContains(layout, edgeCell + intVec) && (desperate || !layout.container.Contains(edgeCell + intVec)))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				TmpCells.Add(edgeCell);
			}
		}
		if (TmpCells.Empty())
		{
			cell = IntVec3.Invalid;
			return false;
		}
		TmpCells.SortBy((IntVec3 x) => x.DistanceTo(targetCenter));
		cell = TmpCells[0];
		TmpCells.Clear();
		return true;
	}

	private static bool AnyRoomContains(StructureLayout layout, IntVec3 cell)
	{
		foreach (LayoutRoom room in layout.Rooms)
		{
			foreach (CellRect rect in room.rects)
			{
				if (rect.Contains(cell))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void MarkConnected(LayoutRoom a, LayoutRoom b)
	{
		a.connections.Add(b);
		b.connections.Add(a);
	}

	private void ReplaceExteriorDoors(LayoutSketch sketch)
	{
		if (ForceExteriorDoor == null)
		{
			return;
		}
		foreach (SketchThing thing in sketch.Things)
		{
			if (thing.def.IsDoor && IsOnExterior(sketch.structureLayout, thing.pos))
			{
				thing.def = ForceExteriorDoor;
				thing.stuff = ForceExteriorDoorStuff;
			}
		}
	}

	private bool IsOnExterior(StructureLayout layout, IntVec3 pos)
	{
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		foreach (IntVec3 intVec in cardinalDirections)
		{
			IntVec3 pos2 = pos + intVec;
			if (!layout.TryGetRoom(pos2, out var _))
			{
				return true;
			}
		}
		return false;
	}

	private void PlaceSurroundingTerrain(LayoutStructureSketch layoutStructureSketch, Map map, Faction faction)
	{
		if (SurroundingTerrain == null)
		{
			return;
		}
		IEnumerable<CellRect> enumerable = layoutStructureSketch.structureLayout.Rooms.SelectMany((LayoutRoom r) => r.rects);
		UsedSurroundingCells.Clear();
		foreach (CellRect item in enumerable)
		{
			int randomInRange = Def.surroundingScatterRange.RandomInRange;
			foreach (IntVec3 item2 in item.DifferenceCells(item.ExpandedBy(randomInRange)))
			{
				if (!item2.InBounds(map) || layoutStructureSketch.AnyRoomContains(item2))
				{
					continue;
				}
				TerrainDef newTerr = SurroundingTerrain;
				if (SurroundingScatterTerrain != null)
				{
					float num = Mathf.Clamp01(Mathf.InverseLerp(0f, randomInRange, layoutStructureSketch.ClosestDistTo(item2)));
					if (Rand.Chance(EasingFunctions.EaseInOutQuad(Mathf.Clamp01(num - 0.2f))))
					{
						newTerr = MapGenUtility.GetNaturalTerrainAt(item2, map);
					}
					else if (Rand.Chance(EasingFunctions.EaseInOutQuad(Mathf.Clamp01(num))))
					{
						newTerr = SurroundingScatterTerrain;
					}
				}
				map.terrainGrid.SetTerrain(item2, newTerr);
				UsedSurroundingCells.Add(item2);
			}
		}
		SurroundingTerrainPlaced(UsedSurroundingCells, layoutStructureSketch, map, faction);
	}

	protected virtual void SurroundingTerrainPlaced(List<IntVec3> cells, LayoutStructureSketch sketch, Map map, Faction faction)
	{
	}
}
