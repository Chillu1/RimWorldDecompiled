using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_TransportRoom : RoomContentsWorker
{
	private static readonly FloatRange ShelvesPer10EdgeCells = new FloatRange(1f, 3f);

	private static readonly IntRange ShelfGroupSizeRange = new IntRange(2, 3);

	private static readonly IntRange RoomSizeRange = new IntRange(5, 12);

	private const float SubroomRatioMaximum = 1.25f;

	private CellRect subroom;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		subroom = GetSubroomRect(map, room);
		SetDefaultRoof(map, room);
		FillSubroom(map, room, subroom);
		SpawnShelves(map, room);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private static void SetDefaultRoof(Map map, LayoutRoom room)
	{
		foreach (CellRect rect in room.rects)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
			}
		}
	}

	protected override bool IsValidCellBase(ThingDef thing, ThingDef stuff, IntVec3 cell, LayoutRoom room, Map map)
	{
		if (!subroom.Contains(cell))
		{
			return base.IsValidCellBase(thing, stuff, cell, room, map);
		}
		return false;
	}

	private static void FillSubroom(Map map, LayoutRoom room, CellRect rect)
	{
		CellRect cellRect = rect.ContractedBy(1);
		foreach (IntVec3 edgeCell in rect.EdgeCells)
		{
			if (edgeCell.GetEdifice(map) == null)
			{
				GenSpawn.Spawn(ThingDefOf.AncientFortifiedWall, edgeCell, map);
			}
		}
		for (int i = 0; i < 4; i++)
		{
			Rot4 rot = new Rot4(i);
			IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot);
			if (room.rects[0].ContractedBy(1).Contains(centerCellOnEdge) && RoomGenUtility.IsGoodForDoor(centerCellOnEdge, map))
			{
				GenSpawn.Spawn(ThingDefOf.AncientBlastDoor, centerCellOnEdge, map);
				break;
			}
		}
		foreach (IntVec3 cell in cellRect.Cells)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
			map.roofGrid.SetRoof(cell, null);
		}
		foreach (IntVec3 corner in cellRect.Corners)
		{
			GenSpawn.Spawn(ThingDefOf.AncientShipBeacon, corner, map);
		}
		if (Rand.Bool)
		{
			GenSpawn.Spawn(ThingDefOf.AncientTransportPod, rect.CenterCell, map);
		}
		else
		{
			GenSpawn.Spawn(ThingDefOf.MalfunctioningTransportPod, rect.CenterCell, map);
		}
	}

	private static CellRect GetSubroomRect(Map map, LayoutRoom room)
	{
		CellRect result = CellRect.Empty;
		int num = Rand.Int;
		for (int i = 0; i < 4; i++)
		{
			Rot4 rot = new Rot4(i + num);
			IntVec3 corner = room.rects[0].GetCorner(rot);
			Rot4 rot2 = ((rot == Rot4.North || rot == Rot4.West) ? Rot4.South : Rot4.North);
			Rot4 rot3 = ((rot == Rot4.North || rot == Rot4.East) ? Rot4.West : Rot4.East);
			int num2 = Mathf.Min(RoomSizeRange.TrueMax, room.rects[0].Width - 4);
			int num3 = Mathf.Min(RoomSizeRange.TrueMax, room.rects[0].Height - 4);
			if ((float)num2 > (float)num3 * 1.25f)
			{
				num2 = (int)((float)num3 * 1.25f);
			}
			else if ((float)num3 > (float)num2 * 1.25f)
			{
				num3 = (int)((float)num2 * 1.25f);
			}
			for (int j = RoomSizeRange.TrueMin; j < num2; j++)
			{
				bool flag = true;
				for (int k = RoomSizeRange.TrueMin; k < num3; k++)
				{
					IntVec3 intVec = rot3.FacingCell * j;
					IntVec3 intVec2 = rot2.FacingCell * k;
					CellRect cellRect = CellRect.FromLimits(corner + intVec + intVec2, corner);
					foreach (IntVec3 edgeCell in cellRect.EdgeCells)
					{
						if (edgeCell.GetDoor(map) != null)
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
					if (cellRect.Area > result.Area && cellRect.Width >= RoomSizeRange.TrueMin && cellRect.Height >= RoomSizeRange.TrueMin)
					{
						result = cellRect;
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}
		if (result.Area == 0)
		{
			int maxExclusive = Mathf.Min(RoomSizeRange.TrueMax, room.rects[0].Width / 2 - 3);
			int maxExclusive2 = Mathf.Min(RoomSizeRange.TrueMax, room.rects[0].Height / 2 - 3);
			result = room.rects[0].CenterCell.RectAbout(new IntVec2(Rand.Range(RoomSizeRange.TrueMin, maxExclusive), Rand.Range(RoomSizeRange.TrueMin, maxExclusive2)));
		}
		return result;
	}

	private void SpawnShelves(Map map, LayoutRoom room)
	{
		List<IntVec3> possibleSpots = new List<IntVec3>(6);
		float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
		int count = Mathf.Max(Mathf.RoundToInt(ShelvesPer10EdgeCells.RandomInRange * num), 1);
		RoomGenUtility.FillAroundEdges(ThingDefOf.Shelf, count, ShelfGroupSizeRange, room, map, Validator, null, 1, 0, null, avoidDoors: true, RotationDirection.Opposite, SpawnAction);
		if (Rand.Bool && possibleSpots.Any())
		{
			IntVec3 intVec = possibleSpots.RandomElement();
			possibleSpots.Remove(intVec);
			SpawnApparel(ThingDefOf.Apparel_VacsuitHelmet, intVec, map);
		}
		if (Rand.Bool && possibleSpots.Any())
		{
			IntVec3 intVec2 = possibleSpots.RandomElement();
			possibleSpots.Remove(intVec2);
			SpawnApparel(ThingDefOf.Apparel_Vacsuit, intVec2, map);
		}
		Thing SpawnAction(IntVec3 pos, Rot4 rot, Map _)
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Shelf, ThingDefOf.Steel);
			GenSpawn.Spawn(thing, pos, map, rot);
			possibleSpots.AddRange(thing.OccupiedRect().Cells);
			return thing;
		}
		bool Validator(IntVec3 pos, Rot4 rot, CellRect rect)
		{
			return IsValidCellBase(ThingDefOf.Shelf, ThingDefOf.Steel, pos, room, map);
		}
	}

	private static void SpawnApparel(ThingDef apparel, IntVec3 cell, Map map)
	{
		Thing thing = ThingMaker.MakeThing(apparel);
		GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
		thing.TrySetForbidden(value: true);
	}
}
