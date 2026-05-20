using Verse;

namespace RimWorld;

public abstract class CompAbilityEffect_WithDuration : CompAbilityEffect
{
	public new CompProperties_AbilityEffectWithDuration Props => (CompProperties_AbilityEffectWithDuration)props;

	public float GetDurationSeconds(Pawn target)
	{
		if (Props.durationSecondsOverride != FloatRange.Zero)
		{
			return Props.durationSecondsOverride.RandomInRange;
		}
		float num = parent.def.GetStatValueAbstract(StatDefOf.Ability_Duration, parent.pawn);
		if (Props.durationMultiplier != null)
		{
			num *= target.GetStatValue(Props.durationMultiplier);
		}
		return num;
	}
}
