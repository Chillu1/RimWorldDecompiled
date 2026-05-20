using Verse;

namespace RimWorld;

public class CompProperties_AbilityAnimalRoar : CompProperties_AbilityEffect
{
	public SimpleCurve chanceFromHearingCurve = new SimpleCurve();

	public CompProperties_AbilityAnimalRoar()
	{
		compClass = typeof(CompAbilityEffect_AnimalRoar);
	}
}
