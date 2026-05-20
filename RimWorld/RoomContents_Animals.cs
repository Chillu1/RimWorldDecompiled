using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_Animals : RoomContentsWorker
{
	private static readonly IntRange AnimalCount = new IntRange(1, 3);

	private static readonly FloatRange WildnessRange = new FloatRange(0.2f, 1f);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		PawnKindDef kind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef k) => k.RaceProps.Animal && k.RaceProps.CanDoHerdMigration).RandomElementByWeight((PawnKindDef x) => Mathf.Lerp(WildnessRange.min, WildnessRange.max, x.race.GetStatValueAbstract(StatDefOf.Wildness)));
		int randomInRange = AnimalCount.RandomInRange;
		PawnGenerationRequest request = new PawnGenerationRequest(kind);
		for (int num = 0; num < randomInRange; num++)
		{
			if (!room.TryGetRandomCellInRoom(map, out var cell, 2))
			{
				break;
			}
			GenSpawn.Spawn(PawnGenerator.GeneratePawn(request), cell, map);
		}
	}
}
