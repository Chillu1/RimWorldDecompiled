using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContentsWorker
{
	private const int DefaultThreatPoints = 300;

	private static readonly List<IntVec3> tmpOccupiedEdges = new List<IntVec3>();

	public LayoutRoomDef RoomDef { get; private set; }

	protected virtual string ThreatSignal { get; set; }

	public void Initialize(LayoutRoomDef roomDef)
	{
		RoomDef = roomDef;
	}

	private bool GenerateRoof(LayoutRoom room)
	{
		if (!RoomDef.noRoof)
		{
			return !room.sketch.layoutDef.noRoof;
		}
		return false;
	}

	private IEnumerable<LayoutScatterParms> ScatterParms(LayoutRoom room)
	{
		foreach (LayoutRoomDef def in room.defs)
		{
			foreach (LayoutScatterParms item in def.scatter)
			{
				yield return item;
			}
		}
		foreach (LayoutRoomDef def2 in room.defs)
		{
			if (!def2.spawnJunk)
			{
				yield break;
			}
		}
		foreach (LayoutScatterParms junkScaterrer in room.sketch.layoutDef.junkScaterrers)
		{
			yield return junkScaterrer;
		}
	}

	private IEnumerable<LayoutScatterTerrainParms> ScatterTerrainParms(LayoutRoom room)
	{
		foreach (LayoutRoomDef def in room.defs)
		{
			foreach (LayoutScatterTerrainParms item in def.scatterTerrain)
			{
				yield return item;
			}
		}
		foreach (LayoutScatterTerrainParms item2 in room.sketch.layoutDef.scatterTerrain)
		{
			yield return item2;
		}
	}

	private IEnumerable<LayoutFillEdgesParms> FillEdgeParms(LayoutRoom room)
	{
		foreach (LayoutRoomDef def in room.defs)
		{
			foreach (LayoutFillEdgesParms fillEdge in def.fillEdges)
			{
				yield return fillEdge;
			}
		}
		foreach (LayoutFillEdgesParms fillEdge2 in room.sketch.layoutDef.fillEdges)
		{
			yield return fillEdge2;
		}
	}

	private IEnumerable<LayoutFillInteriorParms> FillInteriorParms(LayoutRoom room)
	{
		foreach (LayoutRoomDef def in room.defs)
		{
			foreach (LayoutFillInteriorParms item in def.fillInterior)
			{
				yield return item;
			}
		}
		foreach (LayoutFillInteriorParms item2 in room.sketch.layoutDef.fillInterior)
		{
			yield return item2;
		}
	}

	private IEnumerable<LayoutWallAttatchmentParms> WallAttachmentParms(LayoutRoom room)
	{
		if (room.sketch.layoutDef is StructureLayoutDef { wallLampDef: not null } structureLayoutDef)
		{
			yield return new LayoutWallAttatchmentParms
			{
				def = structureLayoutDef.wallLampDef,
				spawnChancePerPosition = structureLayoutDef.wallLampChancePerPosition
			};
		}
		foreach (LayoutRoomDef def in room.defs)
		{
			foreach (LayoutWallAttatchmentParms wallAttachment in def.wallAttachments)
			{
				yield return wallAttachment;
			}
		}
		foreach (LayoutWallAttatchmentParms wallAttachment2 in room.sketch.layoutDef.wallAttachments)
		{
			yield return wallAttachment2;
		}
	}

	private IEnumerable<LayoutPrefabParms> PrefabParms(LayoutRoom room)
	{
		foreach (LayoutRoomDef def in room.defs)
		{
			foreach (LayoutPrefabParms prefab in def.prefabs)
			{
				yield return prefab;
			}
		}
		foreach (LayoutPrefabParms prefab2 in room.sketch.layoutDef.prefabs)
		{
			yield return prefab2;
		}
	}

	private IEnumerable<LayoutPartParms> PartParms(LayoutRoom room)
	{
		foreach (LayoutRoomDef def in room.defs)
		{
			foreach (LayoutPartParms part in def.parts)
			{
				yield return part;
			}
		}
		foreach (LayoutPartParms part2 in room.sketch.layoutDef.parts)
		{
			yield return part2;
		}
	}

	public virtual void PreFillRooms(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
	}

	public virtual void PostFillRooms(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		TrySpawnParts(map, room, faction, threatPoints, post: true);
	}

	public virtual void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		if (RoomDef != null)
		{
			TrySetRoof(map, room);
			RemoveWalls(map, room);
			TryScatterTerrain(map, room);
			TrySpawnParts(map, room, faction, threatPoints, post: false);
			TryPlacePrefabs(map, room, faction);
			TryFillContents(map, room, faction);
			TryScatterContents(map, room, faction);
			TryPlaceWallAttachments(map, room, faction);
		}
	}

	private void TrySetRoof(Map map, LayoutRoom room)
	{
		if (!GenerateRoof(room))
		{
			return;
		}
		foreach (CellRect rect in room.rects)
		{
			foreach (IntVec3 item in rect)
			{
				SpawnRoof(map, item);
			}
		}
	}

	protected void SpawnRoof(Map map, IntVec3 cell)
	{
		RoofDef roof = cell.GetRoof(map);
		if (roof == null || !roof.isNatural)
		{
			map.roofGrid.SetRoof(cell, RoomDef.roofDef ?? RoofDefOf.RoofConstructed);
		}
	}

	private void RemoveWalls(Map map, LayoutRoom room)
	{
		if (!CanRemoveWalls(room))
		{
			return;
		}
		foreach (CellRect rect in room.rects)
		{
			foreach (IntVec3 edgeCell in rect.EdgeCells)
			{
				if (!CanRemoveWall(edgeCell, map, room))
				{
					continue;
				}
				bool flag = true;
				for (int i = 0; i < 4; i++)
				{
					if ((edgeCell + GenAdj.CardinalDirections[i]).GetDoor(map) != null)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				foreach (LayoutRoom room2 in room.sketch.structureLayout.Rooms)
				{
					if (!CanRemoveWalls(room2) && room2.Contains(edgeCell))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Building building = map.edificeGrid[edgeCell];
					if (building != null && building.def.IsWall)
					{
						building.Destroy();
						map.roofGrid.SetRoof(edgeCell, null);
					}
				}
			}
		}
	}

	private void TryScatterTerrain(Map map, LayoutRoom room)
	{
		float num = (float)room.Area / 100f;
		foreach (LayoutScatterTerrainParms item in ScatterTerrainParms(room))
		{
			int a = ((!(item.groupCount != IntRange.Invalid)) ? Mathf.RoundToInt(item.groupsPerHundredCells.RandomInRange * num) : item.groupCount.RandomInRange);
			a = Mathf.Max(a, item.minGroups);
			for (int i = 0; i < a; i++)
			{
				SpawnIrregularTerrainLump(item, map, room);
			}
		}
	}

	private void TrySpawnParts(Map map, LayoutRoom room, Faction faction, float? threatPoints, bool post)
	{
		float num = ((!threatPoints.HasValue) ? 300f : (RoomDef.threatPointsScaleCurve?.Evaluate(threatPoints.Value) ?? threatPoints.Value));
		foreach (LayoutPartParms item in PartParms(room))
		{
			int num2 = 1;
			if (item.def.Worker.FillOnPost == post)
			{
				if (item.countRange != IntRange.Invalid)
				{
					num2 = item.countRange.RandomInRange;
				}
				else if (!Rand.Chance(item.chance))
				{
					continue;
				}
				float threatPoints2 = num;
				if (item.threatPointsRange != IntRange.Invalid)
				{
					threatPoints2 = item.threatPointsRange.RandomInRange;
				}
				for (int i = 0; i < num2; i++)
				{
					item.def.Worker.FillRoom(map, room, faction, threatPoints2);
				}
			}
		}
	}

	private void TryPlacePrefabs(Map map, LayoutRoom room, Faction faction)
	{
		LayoutSketch layoutSketch = room.sketch.layoutSketch;
		float num = (float)room.Area / 100f;
		float num2 = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
		Tuple<ThingDef, ThingDef> wallTuple = new Tuple<ThingDef, ThingDef>(layoutSketch.wall, layoutSketch.wallStuff);
		Tuple<ThingDef, ThingDef> doorTuple = new Tuple<ThingDef, ThingDef>(layoutSketch.door, layoutSketch.doorStuff);
		Dictionary<LayoutPrefabParms, int> dictionary = new Dictionary<LayoutPrefabParms, int>();
		List<LayoutPrefabParms> list = new List<LayoutPrefabParms>();
		foreach (LayoutPrefabParms item in PrefabParms(room))
		{
			list.Add(item);
			int a = ((!(item.countRange != IntRange.Invalid)) ? ((!item.def.edgeOnly) ? Mathf.RoundToInt(item.countPerHundredCells.RandomInRange * num) : Mathf.RoundToInt(item.countPerTenEdgeCells.RandomInRange * num2)) : item.countRange.RandomInRange);
			a = Mathf.Min(Mathf.Max(a, item.minMaxRange.TrueMin), item.minMaxRange.TrueMax);
			dictionary[item] = a;
		}
		int num3 = 0;
		while (dictionary.Any())
		{
			LayoutPrefabParms parms = list[num3];
			num3 = ++num3 % list.Count;
			if (dictionary.ContainsKey(parms))
			{
				if (--dictionary[parms] <= 0)
				{
					dictionary.Remove(parms);
				}
				if (parms.def.edgeOnly)
				{
					RoomGenUtility.FillPrefabsAroundEdges(parms, 1, room, map, Validator, null, 1, avoidDoors: true, faction, OverrideSpawnData);
				}
				else
				{
					RoomGenUtility.FillPrefabs(parms, 1, room, map, Validator, 1, null, alignWithRect: false, snapToGrid: false, faction, OverrideSpawnData);
				}
			}
			bool Validator(IntVec3 cell, Rot4 rot)
			{
				return IsValidPrefabRect(parms, cell, rot, room, map);
			}
		}
		Tuple<ThingDef, ThingDef> OverrideSpawnData(PrefabThingData data)
		{
			if (data.def.IsWall)
			{
				return wallTuple;
			}
			if (data.def.IsDoor)
			{
				return doorTuple;
			}
			return null;
		}
	}

	private void TryPlaceWallAttachments(Map map, LayoutRoom room, Faction faction)
	{
		foreach (LayoutWallAttatchmentParms item in WallAttachmentParms(room))
		{
			LayoutWallAttatchmentParms parms = item;
			if (parms.countRange != IntRange.Invalid)
			{
				RoomGenUtility.SpawnWallAttatchments(parms.def, map, room, parms.countRange, Validator, parms.stuff, faction);
			}
			else
			{
				RoomGenUtility.SpawnWallAttatchments(parms.def, map, room, parms.spawnChancePerPosition, parms.thingsPer10EdgeCells.RandomInRange, Validator, parms.stuff, faction);
			}
			bool Validator(IntVec3 cell, Rot4 rot)
			{
				return IsValidWallAttachmentCell(parms, cell, rot, room, map);
			}
		}
	}

	private void TryFillContents(Map map, LayoutRoom room, Faction faction)
	{
		foreach (LayoutFillEdgesParms item in FillEdgeParms(room))
		{
			LayoutFillEdgesParms parms = item;
			if (parms.countRange != IntRange.Invalid)
			{
				RoomGenUtility.FillAroundEdges(parms.def, parms.countRange.RandomInRange, parms.groupCountRange, room, map, Validator, null, stuff: parms.stuff, padding: parms.padding, contractedBy: parms.contractedBy, avoidDoors: true, rotationDirectionOffset: parms.rotOffset, spawnAction: null, faction: faction);
				continue;
			}
			RoomGenUtility.FillAroundEdges(parms.def, parms.groupsPerTenEdgeCells, parms.groupCountRange, room, map, Validator, null, stuff: parms.stuff, padding: parms.padding, contractedBy: parms.contractedBy, avoidDoors: true, rotationDirectionOffset: parms.rotOffset, spawnAction: null, faction: faction);
			bool Validator(IntVec3 c, Rot4 rot, CellRect rect)
			{
				return IsValidFillEdgeCell(parms, c, rot, room, map);
			}
		}
		foreach (LayoutFillInteriorParms item2 in FillInteriorParms(room))
		{
			LayoutFillInteriorParms parms2 = item2;
			if (parms2.countRange != IntRange.Invalid)
			{
				RoomGenUtility.FillWithPadding(parms2.def, parms2.countRange.RandomInRange, room, map, parms2.fixedRot, Validator2, null, stuff: parms2.stuff, contractedBy: parms2.contractedBy, alignWithRect: parms2.alignWithRect, snapToGrid: parms2.snapToGrid, spawnAction: null, faction: faction);
				continue;
			}
			RoomGenUtility.FillWithPadding(parms2.def, parms2.thingsPerHundredCells, room, map, parms2.fixedRot, Validator2, null, stuff: parms2.stuff, contractedBy: parms2.contractedBy, alignWithRect: parms2.alignWithRect, snapToGrid: parms2.snapToGrid, spawnAction: null, faction: faction);
			bool Validator2(IntVec3 c, Rot4 rot, CellRect rect)
			{
				return IsValidFillInteriorCell(parms2, c, rot, room, map);
			}
		}
	}

	private void TryScatterContents(Map map, LayoutRoom room, Faction faction)
	{
		float num = (float)room.Area / 100f;
		foreach (LayoutScatterParms item in ScatterParms(room))
		{
			int a = ((!(item.groupCount != IntRange.Invalid)) ? Mathf.RoundToInt(item.groupsPerHundredCells.RandomInRange * num) : item.groupCount.RandomInRange);
			a = Mathf.Max(a, item.minGroups);
			for (int i = 0; i < a; i++)
			{
				SpawnIrregularLump(item, map, room, faction);
			}
		}
	}

	private void SpawnIrregularLump(LayoutScatterParms parms, Map map, LayoutRoom room, Faction faction, List<IntVec3> area = null, List<Thing> spawned = null)
	{
		if (room.TryGetRandomCellInRoom(map, out var cell, 2, 0, (IntVec3 c) => IsValidScatterCell(parms, c, room, map)))
		{
			GenSpawn.SpawnIrregularLump(parms.def, cell, map, parms.itemsPerGroup, parms.groupDistRange, WipeMode.Vanish, (IntVec3 c) => IsValidScatterCell(parms, c, room, map), area, spawned, parms.stuff, faction);
		}
	}

	private void SpawnIrregularTerrainLump(LayoutScatterTerrainParms parms, Map map, LayoutRoom room, List<IntVec3> area = null)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 2, 0, Validator))
		{
			return;
		}
		int randomInRange = parms.groupDistRange.RandomInRange;
		int randomInRange2 = parms.itemsPerGroup.RandomInRange;
		int num = 0;
		List<IntVec3> list = GridShapeMaker.IrregularLump(cell, map, randomInRange, Validator);
		for (int i = 0; i < list.Count; i++)
		{
			IntVec3 intVec = list[i];
			if (Rand.DynamicChance(num, randomInRange2, list.Count - i))
			{
				num++;
				map.terrainGrid.SetTerrain(intVec, parms.def);
				area?.Add(intVec);
			}
		}
		bool Validator(IntVec3 c)
		{
			return room.Contains(c);
		}
	}

	protected virtual bool IsValidCellBase(ThingDef thing, ThingDef stuff, IntVec3 cell, LayoutRoom room, Map map)
	{
		TerrainAffordanceDef terrainAffordanceNeed = thing.GetTerrainAffordanceNeed(stuff);
		if (room.Contains(cell, 1) && !cell.IsBuildingInteractionCell(map) && (terrainAffordanceNeed == null || cell.GetAffordances(map).Contains(terrainAffordanceNeed)) && (thing.passability != Traversability.Impassable || cell.GetFirstPawn(map) == null) && cell.GetFirstBuilding(map) == null && (thing.IsFilth || !RoomGenUtility.IsDoorAdjacentTo(cell, map)))
		{
			if (cell.GetTerrain(map).exposesToVacuum)
			{
				return cell.GetRoof(map) != null;
			}
			return true;
		}
		return false;
	}

	protected virtual bool IsValidScatterCell(LayoutScatterParms parms, IntVec3 cell, LayoutRoom room, Map map)
	{
		if (parms.def.passability == Traversability.Impassable && !RoomGenUtility.IsClearAndNotAdjacentToDoor(parms.def, cell, map, Rot4.North))
		{
			return false;
		}
		foreach (Thing thing in cell.GetThingList(map))
		{
			if (parms.def.IsFilth && thing.def.IsFilth)
			{
				return false;
			}
			if (!parms.def.IsFilth && !thing.def.IsFilth)
			{
				return false;
			}
		}
		return IsValidCellBase(parms.def, parms.stuff, cell, room, map);
	}

	protected virtual bool IsValidFillEdgeCell(LayoutFillEdgesParms parms, IntVec3 cell, Rot4 rot, LayoutRoom room, Map map)
	{
		CellRect cellRect = cell.RectAbout(parms.def.Size, rot);
		foreach (IntVec3 item in cellRect)
		{
			if (!IsValidCellBase(parms.def, parms.stuff, item, room, map))
			{
				return false;
			}
		}
		if (parms.def.Fillage != FillCategory.Full)
		{
			return true;
		}
		foreach (IntVec3 edgeCell in cellRect.ExpandedBy(1).EdgeCells)
		{
			if (room.Contains(edgeCell, 2) && !edgeCell.Standable(map))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool IsValidPrefabRect(LayoutPrefabParms parms, IntVec3 cell, Rot4 rot, LayoutRoom room, Map map)
	{
		CellRect cellRect = cell.RectAbout(parms.def.size, rot);
		tmpOccupiedEdges.Clear();
		foreach (var (prefabThingData, center, rot2) in PrefabUtility.GetThings(parms.def, cell, rot))
		{
			foreach (IntVec3 cell2 in center.RectAbout(prefabThingData.def.Size, rot2).Cells)
			{
				if (!IsValidCellBase(prefabThingData.def, prefabThingData.stuff, cell2, room, map))
				{
					return false;
				}
				if (!cellRect.EdgeCells.Contains(cell2) || cell2.Standable(map))
				{
					continue;
				}
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = cell2 + GenAdj.AdjacentCellsAround[i];
					if (!cellRect.Contains(intVec) && !intVec.Standable(map) && !cellRect.ExpandedBy(1).IsCorner(intVec) && !tmpOccupiedEdges.Contains(intVec))
					{
						tmpOccupiedEdges.Add(intVec);
					}
				}
			}
		}
		return true;
	}

	protected virtual bool IsValidFillInteriorCell(LayoutFillInteriorParms parms, IntVec3 cell, Rot4 rot, LayoutRoom room, Map map)
	{
		CellRect cellRect = cell.RectAbout(parms.def.Size, rot);
		foreach (IntVec3 item in cellRect)
		{
			if (!IsValidCellBase(parms.def, parms.stuff, item, room, map))
			{
				return false;
			}
		}
		if (parms.def.Fillage != FillCategory.Full)
		{
			return true;
		}
		foreach (IntVec3 edgeCell in cellRect.ExpandedBy(1).EdgeCells)
		{
			if (room.Contains(edgeCell, 2) && !edgeCell.Standable(map))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool IsValidWallAttachmentCell(LayoutWallAttatchmentParms parms, IntVec3 cell, Rot4 rot, LayoutRoom room, Map map)
	{
		if ((cell + rot.FacingCell).GetEdifice(map)?.def != room.sketch.layoutSketch.wall)
		{
			return false;
		}
		return true;
	}

	protected virtual bool CanRemoveWall(IntVec3 cell, Map map, LayoutRoom room)
	{
		return true;
	}

	private static bool CanRemoveWalls(LayoutRoom room)
	{
		foreach (LayoutRoomDef def in room.defs)
		{
			if (!def.canRemoveBorderWalls)
			{
				return false;
			}
		}
		return true;
	}
}
