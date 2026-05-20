using Verse;

namespace RimWorld;

public class QuestPart_Filter_ThingAnalyzed : QuestPart_Filter
{
	public ThingDef thingDef;

	protected override bool Pass(SignalArgs args)
	{
		if (!(thingDef.CompDefForAssignableFrom<CompAnalyzable>() is CompProperties_CompAnalyzableUnlockResearch compProperties_CompAnalyzableUnlockResearch))
		{
			return false;
		}
		Find.AnalysisManager.TryGetAnalysisProgress(compProperties_CompAnalyzableUnlockResearch.analysisID, out var details);
		return details?.Satisfied ?? false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref thingDef, "thingDef");
	}
}
