using Verse;

namespace RimWorld
{
	public static class DownedRefugeeQuestUtility
	{
		private const float RelationWithColonistWeight = 20f;

		private const float ChanceToRedressWorldPawn = 0.2f;

		public static Pawn GenerateRefugee(int tile)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, GetRandomFactionForRefugee(), PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 20f, forceAddFreeWarmLayerIfNeeded: true, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, null, 1f, null, null, null, null, 0.2f));
			HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
			HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, allowBleedingWounds: false);
			return pawn;
		}

		public static Faction GetRandomFactionForRefugee()
		{
			if (Rand.Chance(0.6f) && Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out Faction faction, tryMedievalOrBetter: true))
			{
				return faction;
			}
			return null;
		}
	}
}
