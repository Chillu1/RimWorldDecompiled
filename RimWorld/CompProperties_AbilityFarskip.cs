using Verse;

namespace RimWorld;

public class CompProperties_AbilityFarskip : CompProperties_AbilityEffect
{
	public IntRange stunTicks;

	public CompProperties_AbilityFarskip()
	{
		compClass = typeof(CompAbilityEffect_Farskip);
	}
}
