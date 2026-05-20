using Verse;

namespace RimWorld;

public class CompAbilityEffect_SocialInteraction : CompAbilityEffect
{
	public new CompProperties_AbilitySocialInteraction Props => (CompProperties_AbilitySocialInteraction)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		if (pawn != null && parent.pawn != pawn)
		{
			parent.pawn.interactions?.TryInteractWith(pawn, Props.interactionDef);
		}
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return Valid(target);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (!Props.canApplyToMentallyBroken && !AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
			{
				return false;
			}
			if (!Props.canApplyToUnconscious && !AbilityUtility.ValidateIsConscious(pawn, throwMessages, parent))
			{
				return false;
			}
		}
		return true;
	}
}
