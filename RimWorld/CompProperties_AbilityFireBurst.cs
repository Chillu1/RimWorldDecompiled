namespace RimWorld
{
	public class CompProperties_AbilityFireBurst : CompProperties_AbilityEffect
	{
		public float radius = 6f;

		public CompProperties_AbilityFireBurst()
		{
			compClass = typeof(CompAbilityEffect_FireBurst);
		}
	}
}
