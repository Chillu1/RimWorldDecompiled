using Verse;

namespace RimWorld;

public class CompAbilityEffect_GiveHediffPsychic : CompAbilityEffect_GiveHediff
{
	public new CompProperties_AbilityGiveHediffPsychic Props => (CompProperties_AbilityGiveHediffPsychic)props;

	protected override bool TryResist(Pawn pawn)
	{
		return Rand.Chance(1f - pawn.GetStatValue(StatDefOf.PsychicSensitivity));
	}
}
