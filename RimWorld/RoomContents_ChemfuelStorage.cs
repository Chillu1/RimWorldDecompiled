using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_ChemfuelStorage : RoomContentsWorker
{
	private static readonly IntRange ChemfuelCountRange = new IntRange(300, 500);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		RoomGenUtility.GenerateRows(ThingDefOf.Shelf, room, map, ThingDefOf.Steel);
		int num = ChemfuelCountRange.RandomInRange;
		int num2 = 99;
		IntVec3 cell;
		while (num > 0 && num2-- > 0 && room.TryGetRandomCellInRoom(map, out cell, 0, 0, (IntVec3 c) => ShelfValidator(map, c, ThingDefOf.Chemfuel), ignoreBuildings: true))
		{
			int a = Rand.Range(25, 50);
			a = Mathf.Min(a, num);
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Chemfuel), cell, map);
			thing.stackCount = a;
			thing.SetForbidden(value: true);
			num -= a;
		}
		base.FillRoom(map, room, faction, threatPoints);
	}

	private bool ShelfValidator(Map map, IntVec3 c, ThingDef itemDef)
	{
		if (!(c.GetFirstThing(map, ThingDefOf.Shelf) is Building_Storage building_Storage))
		{
			return false;
		}
		if (building_Storage.SpaceRemainingFor(itemDef) == 0)
		{
			return false;
		}
		return true;
	}
}
