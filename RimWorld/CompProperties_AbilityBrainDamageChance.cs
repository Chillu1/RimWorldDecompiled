namespace RimWorld
{
	public class CompProperties_AbilityBrainDamageChance : CompProperties_AbilityEffect
	{
		public float brainDamageChance = 0.3f;

		public CompProperties_AbilityBrainDamageChance()
		{
			compClass = typeof(CompAbilityEffect_BrainDamageChance);
		}
	}
}
