using Verse;

namespace RimWorld
{
	public class CompProperties_CausesGameCondition_ClimateAdjuster : CompProperties_CausesGameCondition
	{
		public FloatRange temperatureOffsetRange = new FloatRange(-10f, 10f);

		public CompProperties_CausesGameCondition_ClimateAdjuster()
		{
			compClass = typeof(CompCauseGameCondition_TemperatureOffset);
		}
	}
}
