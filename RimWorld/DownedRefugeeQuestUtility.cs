using RimWorld.Planet;
using Verse;

namespace RimWorld;

public static class DownedRefugeeQuestUtility
{
	private const float RelationWithColonistWeight = 20f;

	private const float ChanceToRedressWorldPawn = 0.2f;

	public static Pawn GenerateRefugee(PlanetTile tile, PawnKindDef pawnKind = null, float chanceForFaction = 0.6f)
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind ?? PawnKindDefOf.SpaceRefugee, GetRandomFactionForRefugee(chanceForFaction), PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 20f, forceAddFreeWarmLayerIfNeeded: true, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, 0.2f));
		HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
		HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, allowBleedingWounds: false);
		return pawn;
	}

	public static Faction GetRandomFactionForRefugee(float chanceForFaction = 0.6f)
	{
		if (Rand.Chance(chanceForFaction) && Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, tryMedievalOrBetter: true))
		{
			return faction;
		}
		return null;
	}
}
