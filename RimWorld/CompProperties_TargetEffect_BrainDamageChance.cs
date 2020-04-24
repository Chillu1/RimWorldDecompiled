using Verse;

namespace RimWorld
{
	public class CompProperties_TargetEffect_BrainDamageChance : CompProperties
	{
		public float brainDamageChance = 0.3f;

		public CompProperties_TargetEffect_BrainDamageChance()
		{
			compClass = typeof(CompTargetEffect_BrainDamageChance);
		}
	}
}
