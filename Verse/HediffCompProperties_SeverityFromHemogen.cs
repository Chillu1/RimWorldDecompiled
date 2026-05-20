namespace Verse
{
	public class HediffCompProperties_SeverityFromHemogen : HediffCompProperties
	{
		public float severityPerHourEmpty;

		public float severityPerHourHemogen;

		public HediffCompProperties_SeverityFromHemogen()
		{
			compClass = typeof(HediffComp_SeverityFromHemogen);
		}
	}
}
