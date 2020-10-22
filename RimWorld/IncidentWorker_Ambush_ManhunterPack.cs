using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
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
			return ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(AdjustedPoints(parms.points), -1, out animalKind);
		}

		protected override List<Pawn> GeneratePawns(IncidentParms parms)
		{
			if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(AdjustedPoints(parms.points), parms.target.Tile, out var animalKind) && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(AdjustedPoints(parms.points), -1, out animalKind))
			{
				Log.Error(string.Concat("Could not find any valid animal kind for ", def, " incident."));
				return new List<Pawn>();
			}
			return ManhunterPackIncidentUtility.GenerateAnimals_NewTmp(animalKind, parms.target.Tile, AdjustedPoints(parms.points));
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
			return string.Format(def.letterText, (caravan != null) ? caravan.Name : "yourCaravan".TranslateSimple(), anyPawn.GetKindLabelPlural()).CapitalizeFirst();
		}
	}
}
