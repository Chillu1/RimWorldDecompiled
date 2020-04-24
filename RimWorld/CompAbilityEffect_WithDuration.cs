using Verse;

namespace RimWorld
{
	public abstract class CompAbilityEffect_WithDuration : CompAbilityEffect
	{
		public new CompProperties_AbilityEffectWithDuration Props => (CompProperties_AbilityEffectWithDuration)props;

		public float GetDurationSeconds(Pawn target)
		{
			float num = parent.def.statBases.GetStatValueFromList(StatDefOf.Ability_Duration, 10f);
			if (Props.durationMultiplier != null)
			{
				num *= target.GetStatValue(Props.durationMultiplier);
			}
			return num;
		}
	}
}
