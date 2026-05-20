using RimWorld;

namespace Verse
{
	public class HediffCompProperties_SeverityFromGas : HediffCompProperties
	{
		public GasType gasType;

		public float severityGasDensityFactor;

		public int intervalTicks = 60;

		public float severityNotExposed;

		public StatDef exposureStatFactor;

		public HediffCompProperties_SeverityFromGas()
		{
			compClass = typeof(HediffComp_SeverityFromGas);
		}
	}
}
