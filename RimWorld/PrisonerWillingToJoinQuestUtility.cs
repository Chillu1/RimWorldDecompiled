using RimWorld.Planet;
using Verse;

namespace RimWorld;

public static class PrisonerWillingToJoinQuestUtility
{
	private const float RelationWithColonistWeight = 75f;

	public static Pawn GeneratePrisoner(PlanetTile tile, Faction hostFaction)
	{
		PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Slave, hostFaction, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 75f, forceAddFreeWarmLayerIfNeeded: true, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: true, worldPawnFactionDoesntMatter: true);
		if (Find.Storyteller.difficulty.ChildrenAllowed)
		{
			request.AllowedDevelopmentalStages |= DevelopmentalStage.Child;
		}
		Pawn pawn = PawnGenerator.GeneratePawn(request);
		pawn.guest.SetGuestStatus(hostFaction, GuestStatus.Prisoner);
		return pawn;
	}
}
