namespace Verse
{
	public class HediffCompProperties_SeverityPerDay : HediffCompProperties
	{
		public float severityPerDay;

		public bool showDaysToRecover;

		public HediffCompProperties_SeverityPerDay()
		{
			compClass = typeof(HediffComp_SeverityPerDay);
		}
	}
}
