using Verse;

namespace RimWorld;

public class Alert_Analyzable_RevenantFlesh : Alert_Analyzable
{
	protected override ThingDef Def => ThingDefOf.RevenantFleshChunk;

	public Alert_Analyzable_RevenantFlesh()
	{
		requireAnomaly = true;
		defaultLabel = "AlertRevenantFlesh".Translate();
		defaultExplanation = "AlertRevenantFleshDesc".Translate();
	}
}
