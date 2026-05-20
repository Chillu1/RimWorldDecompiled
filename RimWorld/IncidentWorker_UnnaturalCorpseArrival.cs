using Verse;

namespace RimWorld;

public class IncidentWorker_UnnaturalCorpseArrival : IncidentWorker
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!(parms.target is Map map))
		{
			return false;
		}
		if (!QuestUtility.TryGetIdealColonist(out var pawn, map, ValidatePawn))
		{
			return false;
		}
		UnnaturalCorpse unnaturalCorpse = AnomalyUtility.MakeUnnaturalCorpse(pawn);
		Find.Anomaly.RegisterUnnaturalCorpse(pawn, unnaturalCorpse);
		RCellFinder.TryFindRandomSpotJustOutsideColony(IntVec3.Invalid, map, out var result);
		GenSpawn.Spawn(unnaturalCorpse, result, map);
		EffecterDefOf.Skip_EntryNoDelay.Spawn(unnaturalCorpse, map).Cleanup();
		SendStandardLetter("UnnaturalCorpseArrivalLabel".Translate(), "UnnaturalCorpseArrivalText".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatSmall, parms, unnaturalCorpse);
		return true;
	}

	protected bool ValidatePawn(Pawn pawn)
	{
		if (!pawn.IsColonist && !pawn.IsSlaveOfColony)
		{
			return false;
		}
		if (pawn.RaceProps.unnaturalCorpseDef == null)
		{
			return false;
		}
		return !Find.Anomaly.PawnHasUnnaturalCorpse(pawn);
	}
}
