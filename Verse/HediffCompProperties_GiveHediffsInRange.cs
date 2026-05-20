using RimWorld;

namespace Verse
{
	public class HediffCompProperties_GiveHediffsInRange : HediffCompProperties
	{
		public float range;

		public TargetingParameters targetingParameters;

		public HediffDef hediff;

		public ThingDef mote;

		public bool hideMoteWhenNotDrafted;

		public float initialSeverity = 1f;

		public bool onlyPawnsInSameFaction = true;

		public HediffCompProperties_GiveHediffsInRange()
		{
			compClass = typeof(HediffComp_GiveHediffsInRange);
		}
	}
}
