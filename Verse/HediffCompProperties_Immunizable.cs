namespace Verse
{
	public class HediffCompProperties_Immunizable : HediffCompProperties
	{
		public float immunityPerDayNotSick;

		public float immunityPerDaySick;

		public float severityPerDayNotImmune;

		public float severityPerDayImmune;

		public FloatRange severityPerDayNotImmuneRandomFactor = new FloatRange(1f, 1f);

		public HediffCompProperties_Immunizable()
		{
			compClass = typeof(HediffComp_Immunizable);
		}
	}
}
