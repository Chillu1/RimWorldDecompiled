using RimWorld;

namespace Verse
{
	public class DamageDefAdditionalHediff
	{
		public HediffDef hediff;

		public float severityPerDamageDealt = 0.1f;

		public float severityFixed;

		public StatDef victimSeverityScaling;

		public bool victimSeverityScalingByInvBodySize;
	}
}
