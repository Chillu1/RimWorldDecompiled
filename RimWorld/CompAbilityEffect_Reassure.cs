using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_Reassure : CompAbilityEffect
{
	public new CompProperties_AbilityReassure Props => (CompProperties_AbilityReassure)props;

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
		return true;
	}

	private float CertaintyGain(Pawn initiator, Pawn recipient)
	{
		return Props.baseCertaintyGain * initiator.GetStatValue(StatDefOf.NegotiationAbility);
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (ModLister.CheckIdeology("Ideoligion certainty"))
		{
			Pawn pawn = parent.pawn;
			Pawn pawn2 = target.Pawn;
			float certaintyGain = CertaintyGain(pawn, pawn2);
			float certainty = pawn2.ideo.Certainty;
			pawn2.ideo.Reassure(certaintyGain);
			Messages.Message(Props.successMessage.Formatted(pawn.Named("INITIATOR"), pawn2.Named("RECIPIENT"), certainty.ToStringPercent().Named("BEFORECERTAINTY"), pawn2.ideo.Certainty.ToStringPercent().Named("AFTERCERTAINTY"), pawn.Ideo.name.Named("IDEO")), new LookTargets(new Pawn[2] { pawn, pawn2 }), MessageTypeDefOf.PositiveEvent);
			PlayLogEntry_Interaction entry = new PlayLogEntry_Interaction(InteractionDefOf.Reassure, parent.pawn, pawn2, null);
			Find.PlayLog.Add(entry);
			if (Props.sound != null)
			{
				Props.sound.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
			}
		}
	}
}
