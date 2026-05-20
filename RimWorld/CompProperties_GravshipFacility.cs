using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_GravshipFacility : CompProperties_Facility
{
	public bool providesFuel;

	public float fuelSavingsPercent;

	public GravshipComponentTypeDef componentTypeDef;

	public bool onlyRequiresLooseConnection;

	public CompProperties_GravshipFacility()
	{
		compClass = typeof(CompGravshipFacility);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
		{
			yield return item;
		}
		if (fuelSavingsPercent > 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_GravshipFuelSavings".Translate(), fuelSavingsPercent.ToStringPercentSigned("F0"), "StatsReport_GravshipFuelSavings_Desc".Translate(), 520);
		}
	}
}
