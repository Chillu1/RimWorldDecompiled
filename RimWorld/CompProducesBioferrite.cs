using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompProducesBioferrite : ThingComp
{
	public CompProperties_ProducesBioferrite Props => (CompProperties_ProducesBioferrite)props;

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		StatDrawEntry statDrawEntry = BioferriteStatDrawEntry(parent as Pawn);
		statDrawEntry.overridesHideStats = true;
		yield return statDrawEntry;
	}

	public static float BioferritePerDay(Pawn pawn)
	{
		CompProducesBioferrite compProducesBioferrite = pawn.TryGetComp<CompProducesBioferrite>();
		if (compProducesBioferrite == null && (!pawn.IsMutant || !pawn.mutant.Def.producesBioferrite))
		{
			return 0f;
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.BioferriteExtracted))
		{
			return 0f;
		}
		float num = compProducesBioferrite?.Props.bioferriteDensity ?? 1f;
		return pawn.BodySize * num;
	}

	public static StatDrawEntry BioferriteStatDrawEntry(Pawn pawn)
	{
		CompProducesBioferrite compProducesBioferrite = pawn.TryGetComp<CompProducesBioferrite>();
		StringBuilder stringBuilder = new StringBuilder("StatsReport_BioferriteGeneration_Desc".Translate());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": 1");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("StatsReport_BodySize".Translate(pawn.BodySize.ToString("F2")) + ": x" + pawn.BodySize.ToStringPercent());
		if (compProducesBioferrite != null)
		{
			stringBuilder.AppendLine("StatsReport_BioferriteDensityMultiplier".Translate() + ": x" + compProducesBioferrite.Props.bioferriteDensity.ToStringPercent());
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.BioferriteExtracted))
		{
			stringBuilder.AppendLine("StatsReport_BioferriteExtractedMultiplier".Translate() + ": x" + 0f.ToStringPercent());
		}
		stringBuilder.AppendLine();
		stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + BioferritePerDay(pawn).ToString("F1"));
		return new StatDrawEntry(StatCategoryDefOf.Containment, "StatsReport_BioferriteGeneration".Translate(), BioferritePerDay(pawn).ToString(), stringBuilder.ToString(), 100);
	}
}
