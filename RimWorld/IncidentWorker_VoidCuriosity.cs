using Verse;

namespace RimWorld;

public class IncidentWorker_VoidCuriosity : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (Find.Anomaly.hasPerformedVoidProvocation)
		{
			return false;
		}
		return PawnsFinder.HomeMaps_FreeColonistsSpawned.Any((Pawn x) => !x.DevelopmentalStage.Baby());
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!PawnsFinder.HomeMaps_FreeColonistsSpawned.TryRandomElement((Pawn x) => !x.DevelopmentalStage.Baby(), out var result))
		{
			return false;
		}
		result.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.VoidCuriosity);
		TaggedString baseLetterText = (ResearchProjectDefOf.BasicPsychicRituals.IsFinished ? "VoidCuriosityText_Researched".Translate(result.Named("PAWN")) : "VoidCuriosityText_NotResearched".Translate(result.Named("PAWN")));
		SendStandardLetter("VoidCuriosityLabel".Translate(), baseLetterText, LetterDefOf.NeutralEvent, parms, result);
		return true;
	}
}
