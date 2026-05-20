namespace Verse
{
	public class HediffCompProperties_GiveHediffLungRot : HediffCompProperties_GiveHediff
	{
		public SimpleCurve mtbOverRotGasExposureCurve;

		public float minSeverity = 0.5f;

		public int mtbCheckDuration = 60;

		public HediffCompProperties_GiveHediffLungRot()
		{
			compClass = typeof(HediffComp_GiveHediffLungRot);
		}
	}
}
