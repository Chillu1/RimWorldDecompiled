using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_ProximityFuse : CompProperties
{
	public ThingDef target;

	public float radius;

	public CompProperties_ProximityFuse()
	{
		compClass = typeof(CompProximityFuse);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (parentDef.tickerType != TickerType.Normal)
		{
			yield return "CompProximityFuse needs tickerType " + TickerType.Rare.ToString() + " or faster, has " + parentDef.tickerType;
		}
		if (parentDef.CompDefFor<CompExplosive>() == null)
		{
			yield return "CompProximityFuse requires a CompExplosive";
		}
	}
}
