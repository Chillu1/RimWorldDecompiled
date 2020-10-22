using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Power : CompProperties
	{
		public bool transmitsPower;

		public float basePowerConsumption;

		public bool shortCircuitInRain;

		public SoundDef soundPowerOn;

		public SoundDef soundPowerOff;

		public SoundDef soundAmbientPowered;

		public SoundDef soundAmbientProducingPower;

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
			{
				yield return item;
			}
			if (basePowerConsumption > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Building, "PowerConsumption".Translate(), basePowerConsumption.ToString("F0") + " W", "Stat_Thing_PowerConsumption_Desc".Translate(), 5000);
			}
		}
	}
}
