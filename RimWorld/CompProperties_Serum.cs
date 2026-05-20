using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Serum : CompProperties
{
	public CompProperties_Serum()
	{
		compClass = typeof(CompSerum);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		HediffCompProperties_DisappearsDisableable hediffCompProperties_DisappearsDisableable = ((req.Def as ThingDef)?.GetCompProperties<CompProperties_UseEffectAddHediff>().hediffDef)?.CompProps<HediffCompProperties_DisappearsDisableable>();
		if (hediffCompProperties_DisappearsDisableable != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Serum, "StatsReport_SerumDuration".Translate(), hediffCompProperties_DisappearsDisableable.disappearsAfterTicks.min.ToStringTicksToPeriod(), "StatsReport_SerumDuration_Desc".Translate(), 1000);
		}
	}
}
