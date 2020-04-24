using Verse;

namespace RimWorld
{
	public class CompProperties_TempControl : CompProperties
	{
		public float energyPerSecond = 12f;

		public float defaultTargetTemperature = 21f;

		public float minTargetTemperature = -50f;

		public float maxTargetTemperature = 50f;

		public float lowPowerConsumptionFactor = 0.1f;

		public CompProperties_TempControl()
		{
			compClass = typeof(CompTempControl);
		}
	}
}
