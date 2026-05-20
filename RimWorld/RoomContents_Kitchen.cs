using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_Kitchen : RoomContentsWorker
{
	private const float FloorSteelPercentage = 0.75f;

	protected virtual FloatRange GroupsPer100Cells => new FloatRange(1f, 2f);

	protected virtual IntRange ItemsPerGroup => new IntRange(1, 3);

	protected virtual IntRange SinksCount => new IntRange(3, 6);

	protected virtual IntRange SinkMaxRange => new IntRange(2, 4);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnEdgeThings(ThingDefOf.AncientMicrowave, map, room);
		SpawnEdgeThings(ThingDefOf.AncientRefrigerator, map, room);
		SpawnEdgeThings(ThingDefOf.AncientStove, map, room);
		SpawnSinks(map, room);
		ScatterTables(map, room);
		foreach (IntVec3 cell in room.Cells)
		{
			if (Rand.Chance(0.75f))
			{
				map.terrainGrid.SetTerrain(cell, TerrainDefOf.AncientTile);
			}
		}
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void ScatterTables(Map map, LayoutRoom room)
	{
		Rot4 value = Rot4.North;
		CellRect boundary = room.Boundary;
		if (boundary.Height > boundary.Width)
		{
			value = Rot4.East;
		}
		RoomGenUtility.FillWithPadding(ThingDefOf.Table1x2c, GetCount(room), room, map, value, null, null, 1, ThingDefOf.Steel);
	}

	private void SpawnSinks(Map map, LayoutRoom room)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 4))
		{
			cell = room.rects[0].CenterCell;
		}
		GenSpawn.SpawnIrregularLump(ThingDefOf.AncientKitchenSink, cell, map, SinksCount, SinkMaxRange, WipeMode.Vanish, (IntVec3 c) => IsValid(c, map, room));
	}

	private void SpawnEdgeThings(ThingDef def, Map map, LayoutRoom room)
	{
		RoomGenUtility.FillAroundEdges(def, GetCount(room), ItemsPerGroup, room, map);
	}

	private int GetCount(LayoutRoom room)
	{
		float num = (float)room.Area / 100f;
		return Mathf.Max(Mathf.RoundToInt(GroupsPer100Cells.RandomInRange * num), 1);
	}

	private bool IsValid(IntVec3 cell, Map map, LayoutRoom room)
	{
		if (!room.Contains(cell, 1))
		{
			return false;
		}
		if (cell.GetEdifice(map) != null)
		{
			return false;
		}
		return true;
	}
}
