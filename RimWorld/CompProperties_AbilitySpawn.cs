using Verse;

namespace RimWorld
{
	public class CompProperties_AbilitySpawn : CompProperties_AbilityEffect
	{
		public ThingDef thingDef;

		public CompProperties_AbilitySpawn()
		{
			compClass = typeof(CompAbilityEffect_Spawn);
		}
	}
}
