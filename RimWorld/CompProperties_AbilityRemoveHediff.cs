using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityRemoveHediff : CompProperties_AbilityEffect
	{
		public HediffDef hediffDef;

		public bool applyToSelf;

		public bool applyToTarget;

		public CompProperties_AbilityRemoveHediff()
		{
			compClass = typeof(CompAbilityEffect_RemoveHediff);
		}
	}
}
