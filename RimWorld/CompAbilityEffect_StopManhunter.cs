using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_StopManhunter : CompAbilityEffect
	{
		public new CompProperties_StopManhunter Props => (CompProperties_StopManhunter)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (ModLister.CheckIdeology("Animal calm"))
			{
				Pawn pawn = target.Pawn;
				pawn.MentalState.RecoverFromState();
				Messages.Message(Props.successMessage.Formatted(parent.pawn.Named("INITIATOR"), pawn.Named("RECIPIENT")), new LookTargets(new Pawn[2] { parent.pawn, pawn }), MessageTypeDefOf.PositiveEvent);
			}
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn pawn = target.Pawn;
			if (pawn == null)
			{
				return false;
			}
			if (!AbilityUtility.ValidateMustBeAnimal(pawn, throwMessages, parent))
			{
				return false;
			}
			if (!AbilityUtility.ValidateIsMaddened(pawn, throwMessages, parent))
			{
				return false;
			}
			if (!AbilityUtility.ValidateIsAwake(pawn, throwMessages, parent))
			{
				return false;
			}
			return true;
		}

		public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (!AbilityUtility.ValidateIsMaddened(target.Pawn, showMessages: false, parent))
			{
				return false;
			}
			return base.CanApplyOn(target, dest);
		}
	}
}
