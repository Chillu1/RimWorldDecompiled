using Verse;

namespace RimWorld
{
	public class CompProperties_CauseHediff_Apparel : CompProperties
	{
		public HediffDef hediff;

		public BodyPartDef part;

		public CompProperties_CauseHediff_Apparel()
		{
			compClass = typeof(CompCauseHediff_Apparel);
		}
	}
}
