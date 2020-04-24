using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_GiveMentalState : CompAbilityEffect
	{
		public new CompProperties_AbilityGiveMentalState Props => (CompProperties_AbilityGiveMentalState)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Pawn pawn = target.Thing as Pawn;
			if (pawn != null && pawn.mindState.mentalStateHandler.TryStartMentalState(Props.stateDef, null, forceWake: true))
			{
				float num = parent.def.statBases.GetStatValueFromList(StatDefOf.Ability_Duration, 10f);
				if (Props.durationMultiplier != null)
				{
					num *= pawn.GetStatValue(Props.durationMultiplier);
				}
				pawn.mindState.mentalStateHandler.CurState.forceRecoverAfterTicks = num.SecondsToTicks();
			}
		}
	}
}
