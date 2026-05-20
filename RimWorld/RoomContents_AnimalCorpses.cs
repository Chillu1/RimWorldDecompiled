using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_AnimalCorpses : RoomContentsWorker
{
	private static readonly FloatRange CorpseAgeDaysRange = new FloatRange(3f, 30f);

	private static readonly IntRange CorpseCountRange = new IntRange(1, 3);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		PawnKindDef kind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef k) => k.RaceProps.Animal && k.RaceProps.CanDoHerdMigration).RandomElementByWeight((PawnKindDef x) => Mathf.Lerp(0.2f, 1f, x.race.GetStatValueAbstract(StatDefOf.Wildness)));
		int randomInRange = CorpseCountRange.RandomInRange;
		for (int num = 0; num < randomInRange; num++)
		{
			SpawnCorpse(map, room, kind);
		}
	}

	private static Corpse SpawnCorpse(Map map, LayoutRoom room, PawnKindDef kind)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 2))
		{
			return null;
		}
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind));
		pawn.health.SetDead();
		Corpse corpse = pawn.MakeCorpse(null, null);
		corpse.Age = Mathf.RoundToInt(CorpseAgeDaysRange.RandomInRange * 60000f);
		corpse.GetComp<CompRottable>().RotProgress += corpse.Age;
		Find.WorldPawns.PassToWorld(pawn);
		return (Corpse)GenSpawn.Spawn(corpse, cell, map);
	}
}
