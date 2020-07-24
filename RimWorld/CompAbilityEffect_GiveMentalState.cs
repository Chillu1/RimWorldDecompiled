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
			if (pawn != null && !pawn.InMentalState && pawn.mindState.mentalStateHandler.TryStartMentalState(Props.stateDef, null, forceWake: true))
			{
				float num = parent.def.statBases.GetStatValueFromList(StatDefOf.Ability_Duration, 10f);
				if (Props.durationMultiplier != null)
				{
					num *= pawn.GetStatValue(Props.durationMultiplier);
				}
				pawn.mindState.mentalStateHandler.CurState.forceRecoverAfterTicks = num.SecondsToTicks();
			}
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null && pawn.InMentalState)
			{
				if (throwMessages)
				{
					Messages.Message("AbilityCantApplyToMentallyBroken".Translate(pawn.LabelShort), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}
	}
}
