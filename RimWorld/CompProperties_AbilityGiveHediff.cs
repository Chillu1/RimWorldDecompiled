using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityGiveHediff : CompProperties_AbilityEffectWithDuration
	{
		public HediffDef hediffDef;

		public bool onlyBrain;

		public bool applyToSelf;

		public bool replaceExisting;
	}
}
