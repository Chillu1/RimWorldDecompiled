namespace Verse
{
	public class HediffCompProperties_GrowthMode : HediffCompProperties
	{
		public float severityPerDayGrowing;

		public float severityPerDayRemission;

		public FloatRange severityPerDayGrowingRandomFactor = new FloatRange(1f, 1f);

		public FloatRange severityPerDayRemissionRandomFactor = new FloatRange(1f, 1f);

		public HediffCompProperties_GrowthMode()
		{
			compClass = typeof(HediffComp_GrowthMode);
		}
	}
}
