using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityRequiresCapacity : AbilityCompProperties
	{
		public PawnCapacityDef capacity;

		public CompProperties_AbilityRequiresCapacity()
		{
			compClass = typeof(CompAbility_RequiresCapacity);
		}
	}
}
