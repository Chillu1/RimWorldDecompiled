using Verse;

namespace RimWorld;

public class RoomContents_StatueDeadlife : RoomContents_DeadBodyLabyrinth
{
	protected override IntRange CorpseRange => new IntRange(1, 3);

	protected override IntRange BloodFilthRange => new IntRange(2, 4);

	protected override void SpawnCorpses(Map map, LayoutRoom room)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 4))
		{
			cell = room.rects[0].CenterCell;
		}
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.GrayStatueDeadlifeDust), cell, map);
		int randomInRange = CorpseRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			if (RCellFinder.TryFindRandomCellNearWith(cell, (IntVec3 c) => ValidCell(c, map), map, out var result, 2, 6))
			{
				SpawnCorpse(result, map).InnerPawn.inventory.DestroyAll();
			}
		}
	}

	private bool ValidCell(IntVec3 cell, Map map)
	{
		return cell.GetFirstThing<Thing>(map) == null;
	}
}
