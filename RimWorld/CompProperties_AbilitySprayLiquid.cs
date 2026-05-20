using Verse;

namespace RimWorld
{
	public class CompProperties_AbilitySprayLiquid : CompProperties_AbilityEffect
	{
		public ThingDef projectileDef;

		public int numCellsToHit;

		public EffecterDef sprayEffecter;

		public CompProperties_AbilitySprayLiquid()
		{
			compClass = typeof(CompAbilityEffect_SprayLiquid);
		}
	}
}
