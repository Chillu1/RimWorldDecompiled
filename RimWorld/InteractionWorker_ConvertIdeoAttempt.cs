using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class InteractionWorker_ConvertIdeoAttempt : InteractionWorker
{
	private enum PawnConversionCategory
	{
		Colonist,
		NPC_Free,
		NPC_Prisoner,
		Slave
	}

	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		if (!ModsConfig.IdeologyActive)
		{
			return 0f;
		}
		if (Find.IdeoManager.classicMode)
		{
			return 0f;
		}
		if (initiator.Ideo == null || !recipient.RaceProps.Humanlike || initiator.Ideo == recipient.Ideo)
		{
			return 0f;
		}
		if (recipient.DevelopmentalStage.Baby())
		{
			return 0f;
		}
		if (initiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
		{
			return 0f;
		}
		return 0.04f * initiator.GetStatValue(StatDefOf.SocialIdeoSpreadFrequencyFactor) * ConversionSelectionFactor(initiator, recipient);
	}

	private float ConversionSelectionFactor(Pawn initiator, Pawn recipient)
	{
		PawnConversionCategory pawnConversionCategory = GetConversionCategory(initiator);
		PawnConversionCategory pawnConversionCategory2 = GetConversionCategory(recipient);
		switch (pawnConversionCategory)
		{
		case PawnConversionCategory.NPC_Free:
			switch (pawnConversionCategory2)
			{
			case PawnConversionCategory.Colonist:
				return 0.5f;
			case PawnConversionCategory.NPC_Free:
				return 0.25f;
			case PawnConversionCategory.NPC_Prisoner:
				return 0.25f;
			case PawnConversionCategory.Slave:
				return 0.5f;
			}
			break;
		case PawnConversionCategory.NPC_Prisoner:
			switch (pawnConversionCategory2)
			{
			case PawnConversionCategory.Colonist:
				return 0.25f;
			case PawnConversionCategory.NPC_Free:
				return 0.25f;
			case PawnConversionCategory.NPC_Prisoner:
				return 0.5f;
			case PawnConversionCategory.Slave:
				return 0.5f;
			}
			break;
		case PawnConversionCategory.Slave:
			return 0.5f;
		}
		return 1f;
		static PawnConversionCategory GetConversionCategory(Pawn pawn)
		{
			if (pawn.IsSlave)
			{
				return PawnConversionCategory.Slave;
			}
			if (pawn.IsColonist || pawn.IsPrisonerOfColony)
			{
				return PawnConversionCategory.Colonist;
			}
			if (pawn.IsPrisoner)
			{
				return PawnConversionCategory.NPC_Prisoner;
			}
			return PawnConversionCategory.NPC_Free;
		}
	}

	public static float CertaintyReduction(Pawn initiator, Pawn recipient)
	{
		float num = 0.06f * initiator.GetStatValue(StatDefOf.ConversionPower) * recipient.GetStatValue(StatDefOf.CertaintyLossFactor) * ConversionUtility.ConversionPowerFactor_MemesVsTraits(initiator, recipient) * ReliquaryUtility.GetRelicConvertPowerFactorForPawn(initiator) * Find.Storyteller.difficulty.CertaintyReductionFactor(initiator, recipient);
		Precept_Role precept_Role = recipient.Ideo?.GetRole(recipient);
		if (precept_Role != null)
		{
			num *= precept_Role.def.certaintyLossFactor;
		}
		return num;
	}

	public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
	{
		letterLabel = null;
		letterText = null;
		letterDef = null;
		lookTargets = null;
		Ideo ideo = recipient.Ideo;
		Precept_Role role = ideo.GetRole(recipient);
		float certainty = recipient.ideo.Certainty;
		if (recipient.ideo.IdeoConversionAttempt(CertaintyReduction(initiator, recipient), initiator.Ideo))
		{
			if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
			{
				letterLabel = "LetterLabelConvertIdeoAttempt_Success".Translate();
				letterText = "LetterConvertIdeoAttempt_Success".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"), initiator.Ideo.Named("IDEO"), ideo.Named("OLDIDEO")).Resolve();
				letterDef = LetterDefOf.PositiveEvent;
				lookTargets = new LookTargets(initiator, recipient);
				if (role != null)
				{
					letterText = letterText + "\n\n" + "LetterRoleLostLetterIdeoChangedPostfix".Translate(recipient.Named("PAWN"), role.Named("ROLE"), ideo.Named("OLDIDEO")).Resolve();
				}
			}
			extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptSuccess);
			return;
		}
		float num = (recipient.interactions.SocialFightPossible(initiator) ? 0.02f : 0f);
		float num2 = Rand.Value * (0.97999996f + num);
		if (num2 < 0.78f || recipient.IsPrisoner)
		{
			extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFail);
		}
		else if (num2 < 0.97999996f)
		{
			if (recipient.needs.mood != null)
			{
				if (PawnUtility.ShouldSendNotificationAbout(recipient))
				{
					Messages.Message("MessageFailedConvertIdeoAttempt".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"), certainty.ToStringPercent().Named("CERTAINTYBEFORE"), recipient.ideo.Certainty.ToStringPercent().Named("CERTAINTYAFTER")), recipient, MessageTypeDefOf.NeutralEvent);
				}
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedConvertIdeoAttemptResentment, initiator);
			}
			extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFailResentment);
		}
		else
		{
			recipient.interactions.StartSocialFight(initiator, "MessageFailedConvertIdeoAttemptSocialFight");
			extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFailSocialFight);
		}
	}
}
