using Verse;

namespace RimWorld;

public class CompProperties_DropMechs : CompProperties_EffectWithDest
{
	public FloatRange points;

	public CompProperties_DropMechs()
	{
		compClass = typeof(CompAbilityEffect_DropMechs);
	}
}
