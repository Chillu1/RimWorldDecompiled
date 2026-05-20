namespace Verse
{
	public class HediffCompProperties_Pollution : HediffCompProperties
	{
		public float pollutedSeverity;

		public float unpollutedSeverity;

		public int interval = 1;

		public HediffCompProperties_Pollution()
		{
			compClass = typeof(HediffComp_Pollution);
		}
	}
}
