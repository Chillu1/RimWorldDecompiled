using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_FacilityInUse : CompProperties
	{
		public float? inUsePowerConsumption;

		public EffecterDef effectInUse;

		public CompProperties_FacilityInUse()
		{
			compClass = typeof(CompFacilityInUse);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			if (parentDef.tickerType == TickerType.Never)
			{
				yield return $"CompProperties_FacilityInUse has parent {parentDef} with tickerType=Never";
			}
			if (effectInUse != null && parentDef.tickerType != TickerType.Normal)
			{
				yield return $"CompProperties_FacilityInUse has effectInUse but parent {parentDef} has tickerType!=Normal";
			}
		}
	}
}
