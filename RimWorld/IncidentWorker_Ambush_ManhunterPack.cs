using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class IncidentWorker_Ambush_ManhunterPack : IncidentWorker_Ambush
{
	private const float ManhunterAmbushPointsFactor = 0.75f;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		PawnKindDef animalKind;
		return AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(AdjustedPoints(parms.points), PlanetTile.Invalid, out animalKind);
	}

	protected override List<Pawn> GeneratePawns(IncidentParms parms)
	{
		if (!AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(AdjustedPoints(parms.points), parms.target.Tile, out var animalKind) && !AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(AdjustedPoints(parms.points), PlanetTile.Invalid, out animalKind))
		{
			Log.Error("Could not find any valid animal kind for " + def?.ToString() + " incident.");
			return new List<Pawn>();
		}
		return AggressiveAnimalIncidentUtility.GenerateAnimals(animalKind, parms.target.Tile, AdjustedPoints(parms.points));
	}

	protected override void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
	{
		for (int i = 0; i < generatedPawns.Count; i++)
		{
			generatedPawns[i].health.AddHediff(HediffDefOf.Scaria);
			generatedPawns[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
		}
	}

	private float AdjustedPoints(float basePoints)
	{
		return basePoints * 0.75f;
	}

	protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
	{
		Caravan caravan = parms.target as Caravan;
		return def.letterText.Formatted((caravan != null) ? caravan.Name : "yourCaravan".TranslateSimple(), anyPawn.GetKindLabelPlural()).CapitalizeFirst();
	}
}
