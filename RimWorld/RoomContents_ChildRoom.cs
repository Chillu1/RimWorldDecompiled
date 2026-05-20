using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_ChildRoom : RoomContents_DeadBodyLabyrinth
{
	private static readonly FloatRange YearsAgeChild = new FloatRange(2f, 3f);

	private static readonly FloatRange YearsAgeAdult = new FloatRange(20f, 40f);

	private static readonly IntRange OtherAgeOffsetDays = new IntRange(-2, 2);

	protected override FloatRange CorpseAgeDaysRange => new FloatRange(30f, 60f);

	protected override IntRange BloodFilthRange => new IntRange(3, 6);

	protected override void SpawnCorpses(Map map, LayoutRoom room)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 1))
		{
			return;
		}
		int num = Mathf.RoundToInt(CorpseAgeDaysRange.RandomInRange * 60000f);
		SpawnCorpse(cell, PawnKindDefOf.Villager, num, map, YearsAgeChild.RandomInRange, forceNoGear: true);
		foreach (IntVec3 item in GenAdjFast.AdjacentCellsCardinal(cell))
		{
			if (item.Standable(map))
			{
				num += Mathf.RoundToInt(OtherAgeOffsetDays.RandomInRange * 60000);
				SpawnCorpse(item, PawnKindDefOf.Villager, num, map, YearsAgeAdult.RandomInRange);
				break;
			}
		}
		if (ModsConfig.BiotechActive && RCellFinder.TryFindRandomCellNearWith(cell, (IntVec3 c) => room.Contains(c, 1), map, out var result, 2, 10))
		{
			FilthMaker.TryMakeFilth(result, map, ThingDefOf.Filth_Floordrawing);
		}
	}
}
