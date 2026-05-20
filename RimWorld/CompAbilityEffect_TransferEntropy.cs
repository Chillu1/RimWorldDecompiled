using Verse;

namespace RimWorld;

public class CompAbilityEffect_TransferEntropy : CompAbilityEffect
{
	public new CompProperties_AbilityTransferEntropy Props => (CompProperties_AbilityTransferEntropy)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			Pawn pawn2 = parent.pawn;
			if (Props.targetReceivesEntropy)
			{
				pawn.psychicEntropy.TryAddEntropy(pawn2.psychicEntropy.EntropyValue, pawn2, scale: false, overLimit: true);
			}
			pawn2.psychicEntropy.RemoveAllEntropy();
			MoteMaker.MakeInteractionOverlay(ThingDefOf.Mote_PsychicLinkPulse, parent.pawn, pawn);
		}
		else
		{
			Log.Error("CompAbilityEffect_TransferEntropy is only applicable to pawns.");
		}
	}

	public override bool GizmoDisabled(out string reason)
	{
		if (parent.pawn.psychicEntropy.EntropyValue <= 0f)
		{
			reason = "AbilityNoEntropyToDump".Translate();
			return true;
		}
		return base.GizmoDisabled(out reason);
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null && !AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
		{
			return false;
		}
		return true;
	}
}
