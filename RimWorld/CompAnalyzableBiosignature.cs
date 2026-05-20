using Verse;

namespace RimWorld;

public class CompAnalyzableBiosignature : CompAnalyzable
{
	public int biosignature;

	public new CompProperties_CompAnalyzableBiosignature Props => (CompProperties_CompAnalyzableBiosignature)props;

	public override int AnalysisID => biosignature;

	public override NamedArgument? ExtraNamedArg => AnomalyUtility.GetBiosignatureName(biosignature).Colorize(ColoredText.NameColor).Named("BIOSIGNATURE");

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref biosignature, "biosignature", 0);
	}
}
