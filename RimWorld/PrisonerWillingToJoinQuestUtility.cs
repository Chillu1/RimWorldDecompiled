using Verse;

namespace RimWorld
{
	public static class PrisonerWillingToJoinQuestUtility
	{
		private const float RelationWithColonistWeight = 75f;

		public static Pawn GeneratePrisoner(int tile, Faction hostFaction)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Slave, hostFaction, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 75f, forceAddFreeWarmLayerIfNeeded: true, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: true, worldPawnFactionDoesntMatter: true));
			pawn.guest.SetGuestStatus(hostFaction, prisoner: true);
			return pawn;
		}
	}
}
