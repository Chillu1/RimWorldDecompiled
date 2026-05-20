using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_FuelRoom : RoomContentsWorker
{
	private static readonly IntRange FuelNodeRange = new IntRange(1, 2);

	private static readonly FloatRange AncientGeneratorsPer100Cells = new FloatRange(3f, 5f);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		float num = (float)room.Area / 100f;
		int count = Mathf.Max(Mathf.RoundToInt(AncientGeneratorsPer100Cells.RandomInRange * num), 1);
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientGenerator, count, room, map, Rot4.North);
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientFuelNode, FuelNodeRange.RandomInRange, room, map, Rot4.North);
		base.FillRoom(map, room, faction, threatPoints);
	}
}
