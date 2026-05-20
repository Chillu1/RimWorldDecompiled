using System.Collections.Generic;

namespace RimWorld
{
	public class CompProperties_AbilityGiveRandomHediff : CompProperties_AbilityEffect
	{
		public List<HediffOption> options;

		public bool allowDuplicates;

		public CompProperties_AbilityGiveRandomHediff()
		{
			compClass = typeof(CompAbilityEffect_GiveRandomHediff);
		}
	}
}
