using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_DeathrestBindable : CompProperties
{
	public int stackLimit;

	public bool countsTowardsBuildingLimit = true;

	public bool displayTimeActive = true;

	public float deathrestEffectivenessFactor = 1f;

	public bool mustBeLayingInToBind;

	public float hemogenLimitOffset;

	public HediffDef hediffToApply;

	public SoundDef soundWorking;

	public SoundDef soundStart;

	public SoundDef soundEnd;

	public CompProperties_DeathrestBindable()
	{
		compClass = typeof(CompDeathrestBindable);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		if (!Mathf.Approximately(deathrestEffectivenessFactor, 1f))
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_DeathrestEffectiveness".Translate(), deathrestEffectivenessFactor.ToStringPercent(), "StatsReport_DeathrestEffectiveness_Desc".Translate(), 900);
		}
		if (!mustBeLayingInToBind)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_DeathrestConnectionLimit".Translate(), (stackLimit <= 0) ? "Unlimited".Translate().ToString() : stackLimit.ToString(), "StatsReport_DeathrestConnectionLimit_Desc".Translate(), 910);
		}
	}
}
