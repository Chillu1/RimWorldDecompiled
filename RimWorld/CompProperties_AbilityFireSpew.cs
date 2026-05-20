using Verse;

namespace RimWorld;

public class CompProperties_AbilityFireSpew : CompProperties_AbilityEffect
{
	public float range;

	public float lineWidthEnd;

	public ThingDef filthDef;

	public int damAmount = -1;

	public EffecterDef effecterDef;

	public bool canHitFilledCells;

	public CompProperties_AbilityFireSpew()
	{
		compClass = typeof(CompAbilityEffect_FireSpew);
	}
}
