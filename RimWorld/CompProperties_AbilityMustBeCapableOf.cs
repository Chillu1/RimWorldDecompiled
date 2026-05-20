using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityMustBeCapableOf : CompProperties_AbilityEffect
	{
		public WorkTags workTags;

		public CompProperties_AbilityMustBeCapableOf()
		{
			compClass = typeof(CompAbilityEffect_MustBeCapableOf);
		}
	}
}
