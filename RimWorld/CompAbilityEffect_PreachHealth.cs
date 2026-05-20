using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_PreachHealth : CompAbilityEffect
	{
		public new CompProperties_PreachHealth Props => (CompProperties_PreachHealth)props;

		public override bool HideTargetPawnTooltip => true;

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn pawn = target.Pawn;
			if (pawn == null)
			{
				return false;
			}
			if (!AbilityUtility.ValidateMustBeHuman(pawn, throwMessages, parent))
			{
				return false;
			}
			if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
			{
				return false;
			}
			if (!AbilityUtility.ValidateSameIdeo(parent.pawn, pawn, throwMessages, parent))
			{
				return false;
			}
			if (!AbilityUtility.ValidateSickOrInjured(pawn, throwMessages, parent))
			{
				return false;
			}
			return true;
		}
	}
}
