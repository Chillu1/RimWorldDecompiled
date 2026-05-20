using Verse;

namespace RimWorld;

public class CompBiosignatureOwner : ThingComp
{
	public int biosignature;

	private string biosignatureName;

	public CompProperties_BiosignatureOwner Props => (CompProperties_BiosignatureOwner)props;

	public string BiosignatureName => biosignatureName ?? (biosignatureName = AnomalyUtility.GetBiosignatureName(biosignature));

	public override string CompInspectStringExtra()
	{
		string arg;
		if (Props.requiresAnalysis)
		{
			Find.AnalysisManager.TryGetAnalysisProgress(biosignature, out var details);
			arg = ((details == null || details.timesDone == 0) ? "Unknown".Translate().Resolve().CapitalizeFirst() : string.Format("{0} ({1} x{2})", BiosignatureName, "Analyzed".Translate(), details.timesDone));
		}
		else
		{
			arg = BiosignatureName;
		}
		return string.Format("{0}: {1}", "Biosignature".Translate(), arg);
	}

	public override void PostPostMake()
	{
		biosignature = Rand.Int;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref biosignature, "biosignature", 0);
	}
}
