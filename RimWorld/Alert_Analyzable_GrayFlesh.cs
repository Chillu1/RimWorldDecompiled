using Verse;

namespace RimWorld;

public class Alert_Analyzable_GrayFlesh : Alert_Analyzable
{
	protected override ThingDef Def => ThingDefOf.GrayFleshSample;

	public Alert_Analyzable_GrayFlesh()
	{
		requireAnomaly = true;
		defaultLabel = "AlertGrayFleshSample".Translate();
		defaultExplanation = "AlertGrayFleshSampleDesc".Translate();
	}
}
