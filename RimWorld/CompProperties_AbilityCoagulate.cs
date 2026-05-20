using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityCoagulate : CompProperties_AbilityEffect
	{
		public FloatRange tendQualityRange;

		public CompProperties_AbilityCoagulate()
		{
			compClass = typeof(CompAbilityEffect_Coagulate);
		}
	}
}
