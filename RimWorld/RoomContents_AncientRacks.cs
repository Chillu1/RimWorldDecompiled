using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_AncientRacks : RoomContentsWorker
{
	protected virtual FloatRange ShelvesPer100Cells => new FloatRange(20f, 40f);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		float num = (float)room.Area / 100f;
		int count = Mathf.Max(Mathf.RoundToInt(ShelvesPer100Cells.RandomInRange * num), 1);
		Rot4 value = Rot4.North;
		CellRect boundary = room.Boundary;
		if (boundary.Height > boundary.Width)
		{
			value = Rot4.East;
		}
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientSystemRack, count, room, map, value);
		base.FillRoom(map, room, faction, threatPoints);
	}
}
