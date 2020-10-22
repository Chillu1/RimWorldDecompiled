using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_GiveInspiration : CompAbilityEffect
	{
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				InspirationDef randomAvailableInspirationDef = pawn.mindState.inspirationHandler.GetRandomAvailableInspirationDef();
				if (randomAvailableInspirationDef != null)
				{
					base.Apply(target, dest);
					pawn.mindState.inspirationHandler.TryStartInspiration_NewTemp(randomAvailableInspirationDef, "LetterPsychicInspiration".Translate(pawn.Named("PAWN"), parent.pawn.Named("CASTER")));
				}
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
				if (!AbilityUtility.ValidateNoInspiration(pawn, throwMessages))
				{
					return false;
				}
				if (!AbilityUtility.ValidateCanGetInspiration(pawn, throwMessages))
				{
					return false;
				}
			}
			return base.Valid(target, throwMessages);
		}
	}
}
