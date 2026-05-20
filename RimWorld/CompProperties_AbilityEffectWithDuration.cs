using Verse;

namespace RimWorld;

public class CompProperties_AbilityEffectWithDuration : CompProperties_AbilityEffect
{
	public StatDef durationMultiplier;

	public FloatRange durationSecondsOverride = FloatRange.Zero;
}
