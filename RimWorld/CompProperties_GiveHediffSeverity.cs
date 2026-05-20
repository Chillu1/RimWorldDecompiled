using Verse;

namespace RimWorld
{
	public class CompProperties_GiveHediffSeverity : CompProperties
	{
		public HediffDef hediff;

		public float range;

		public float severityPerSecond;

		public bool drugExposure;

		public ChemicalDef chemical;

		public bool allowMechs = true;

		public CompProperties_GiveHediffSeverity()
		{
			compClass = typeof(CompGiveHediffSeverity);
		}
	}
}
