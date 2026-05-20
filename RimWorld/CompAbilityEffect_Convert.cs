using System.Text;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_Convert : CompAbilityEffect
{
	public new CompProperties_AbilityConvert Props => (CompProperties_AbilityConvert)props;

	public override bool HideTargetPawnTooltip => true;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (ModLister.CheckIdeology("Ideoligion conversion"))
		{
			Pawn pawn = parent.pawn;
			Pawn pawn2 = target.Pawn;
			float certaintyReduction = InteractionWorker_ConvertIdeoAttempt.CertaintyReduction(pawn, pawn2) * Props.convertPowerFactor;
			float certainty = pawn2.ideo.Certainty;
			if (pawn2.ideo.IdeoConversionAttempt(certaintyReduction, pawn.Ideo))
			{
				pawn2.ideo.SetIdeo(parent.pawn.Ideo);
				Messages.Message(Props.successMessage.Formatted(pawn.Named("INITIATOR"), pawn2.Named("RECIPIENT"), pawn.Ideo.name.Named("IDEO")), new LookTargets(new Pawn[2] { pawn, pawn2 }), MessageTypeDefOf.PositiveEvent);
				PlayLogEntry_Interaction entry = new PlayLogEntry_Interaction(InteractionDefOf.Convert_Success, parent.pawn, pawn2, null);
				Find.PlayLog.Add(entry);
			}
			else
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(Props.failedThoughtInitiator, pawn2);
				pawn2.needs.mood.thoughts.memories.TryGainMemory(Props.failedThoughtRecipient, pawn);
				Messages.Message(Props.failMessage.Formatted(pawn.Named("INITIATOR"), pawn2.Named("RECIPIENT"), pawn.Ideo.name.Named("IDEO"), certainty.ToStringPercent().Named("CERTAINTYBEFORE"), pawn2.ideo.Certainty.ToStringPercent().Named("CERTAINTYAFTER")), new LookTargets(new Pawn[2] { pawn, pawn2 }), MessageTypeDefOf.NeutralEvent);
				PlayLogEntry_Interaction entry2 = new PlayLogEntry_Interaction(InteractionDefOf.Convert_Failure, parent.pawn, pawn2, null);
				Find.PlayLog.Add(entry2);
			}
			if (Props.sound != null)
			{
				Props.sound.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
			}
		}
	}

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
		if (!AbilityUtility.ValidateMustNotBeBaby(pawn, throwMessages, parent))
		{
			return false;
		}
		if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
		{
			return false;
		}
		if (!AbilityUtility.ValidateNotSameIdeo(parent.pawn, pawn, throwMessages, parent))
		{
			return false;
		}
		if (!AbilityUtility.ValidateIsConscious(pawn, throwMessages, parent))
		{
			return false;
		}
		return true;
	}

	public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
	{
		if (target.Pawn == null || !Valid(target))
		{
			return null;
		}
		Pawn pawn = parent.pawn;
		Pawn pawn2 = target.Pawn;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("AbilityIdeoConvertBreakdownLabel".Translate().CapitalizeFirst() + ": " + (InteractionWorker_ConvertIdeoAttempt.CertaintyReduction(pawn, pawn2) * Props.convertPowerFactor).ToStringPercent());
		stringBuilder.AppendLine();
		stringBuilder.AppendInNewLine("Factors".Translate() + ":");
		stringBuilder.AppendInNewLine(" -  " + "Base".Translate().CapitalizeFirst() + ": " + 0.06f.ToStringPercent());
		stringBuilder.AppendInNewLine(" -  " + "AbilityIdeoConvertBreakdownUsingAbility".Translate(parent.def.LabelCap.Named("ABILITY")) + ": " + Props.convertPowerFactor.ToStringPercent());
		float statValue = pawn.GetStatValue(StatDefOf.ConversionPower);
		if (statValue != 1f)
		{
			stringBuilder.AppendInNewLine(" -  " + "AbilityIdeoConvertBreakdownConversionPower".Translate(pawn.Named("PAWN")) + ": " + statValue.ToStringPercent());
		}
		TaggedString certaintyReductionFactorsDescription = ConversionUtility.GetCertaintyReductionFactorsDescription(pawn2);
		if (!certaintyReductionFactorsDescription.NullOrEmpty())
		{
			stringBuilder.AppendInNewLine(" -  " + certaintyReductionFactorsDescription);
		}
		Precept_Role precept_Role = pawn2.Ideo?.GetRole(pawn2);
		if (precept_Role != null && precept_Role.def.certaintyLossFactor != 1f)
		{
			stringBuilder.AppendInNewLine(" -  " + "AbilityIdeoConvertBreakdownRole".Translate(pawn2.Named("PAWN"), precept_Role.Named("ROLE")) + ": " + precept_Role.def.certaintyLossFactor.ToStringPercent());
		}
		ReliquaryUtility.GetRelicConvertPowerFactorForPawn(pawn, stringBuilder);
		ConversionUtility.ConversionPowerFactor_MemesVsTraits(pawn, pawn2, stringBuilder);
		return stringBuilder.ToString();
	}
}
