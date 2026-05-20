using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityReassure : CompProperties_AbilityEffect
	{
		[MustTranslate]
		public string successMessage;

		public float baseCertaintyGain = 0.075f;

		public CompProperties_AbilityReassure()
		{
			compClass = typeof(CompAbilityEffect_Reassure);
		}
	}
}
