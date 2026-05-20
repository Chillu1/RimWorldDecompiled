using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_OperatingRoom : RoomContentsWorker
{
	private const float FloorSteelPercentage = 0.75f;

	protected virtual FloatRange ItemsPer100Cells => new FloatRange(2f, 4f);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		Rot4 value = Rot4.North;
		CellRect boundary = room.Boundary;
		if (boundary.Height > boundary.Width)
		{
			value = Rot4.East;
		}
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientOperatingTable, GetCount(room), room, map, value);
		foreach (IntVec3 cell in room.Cells)
		{
			if (Rand.Chance(0.75f))
			{
				map.terrainGrid.SetTerrain(cell, TerrainDefOf.AncientTile);
			}
		}
		base.FillRoom(map, room, faction, threatPoints);
	}

	private int GetCount(LayoutRoom room)
	{
		float num = (float)room.Area / 100f;
		return Mathf.Max(Mathf.RoundToInt(ItemsPer100Cells.RandomInRange * num), 1);
	}
}
