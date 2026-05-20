using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_IndustrialStorage : RoomContentsWorker
{
	protected virtual FloatRange ContainersPer100Cells => new FloatRange(3f, 5f);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		float num = (float)room.Area / 100f;
		int count = Mathf.Max(Mathf.RoundToInt(ContainersPer100Cells.RandomInRange * num), 1);
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientLargeContainer, count, room, map);
		base.FillRoom(map, room, faction, threatPoints);
	}
}
