using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class InteractionWorker_RomanceAttempt : InteractionWorker
{
	private const float MinRomanceChanceForRomanceAttempt = 0.15f;

	public const int MinOpinionForRomanceAttempt = 5;

	private const float BaseSelectionWeight = 1.15f;

	private const float BaseSuccessChance = 0.6f;

	public const float TryRomanceSuccessChance = 1f;

	private const int TryRomanceCooldownTicks = 900000;

	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		if (TutorSystem.TutorialMode)
		{
			return 0f;
		}
		if (initiator.DevelopmentalStage.Juvenile() || recipient.DevelopmentalStage.Juvenile())
		{
			return 0f;
		}
		if (initiator.Inhumanized())
		{
			return 0f;
		}
		if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
		{
			return 0f;
		}
		float num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
		if (num < 0.15f)
		{
			return 0f;
		}
		float num2 = 5f;
		int num3 = initiator.relations.OpinionOf(recipient);
		if ((float)num3 < num2)
		{
			return 0f;
		}
		if ((float)recipient.relations.OpinionOf(initiator) < num2)
		{
			return 0f;
		}
		float num4 = 1f;
		if (!new HistoryEvent(initiator.GetHistoryEventForLoveRelationCountPlusOne(), initiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, allowDead: false);
			if (pawn != null)
			{
				float value = initiator.relations.OpinionOf(pawn);
				num4 = Mathf.InverseLerp(50f, -50f, value);
			}
		}
		float num5 = (initiator.story.traits.HasTrait(TraitDefOf.Gay) ? 1f : ((initiator.gender == Gender.Female) ? 0.15f : 1f));
		float num6 = Mathf.InverseLerp(0.15f, 1f, num);
		float num7 = Mathf.InverseLerp(num2, 100f, num3);
		float num8 = ((initiator.gender == recipient.gender) ? ((!initiator.story.traits.HasTrait(TraitDefOf.Gay) || !recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f) : ((initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f));
		return 1.15f * num5 * num6 * num7 * num4 * num8;
	}

	public static float SuccessChance(Pawn initiator, Pawn recipient, float baseChance = 0.6f)
	{
		if (recipient.Inhumanized())
		{
			return 0f;
		}
		if (CanCreatePsychicBondBetween(initiator, recipient))
		{
			if (initiator.IsQuestHelper() || recipient.IsQuestHelper())
			{
				return 0f;
			}
			return 1f;
		}
		return Mathf.Clamp01(((ModsConfig.BiotechActive && initiator.CurJobDef == JobDefOf.TryRomance) ? 1f : baseChance) * recipient.relations.SecondaryRomanceChanceFactor(initiator) * OpinionFactor(initiator, recipient) * PartnerFactor(initiator, recipient));
	}

	private static float PartnerFactor(Pawn initiator, Pawn recipient)
	{
		float num = 1f;
		if (!new HistoryEvent(recipient.GetHistoryEventForLoveRelationCountPlusOne(), recipient.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			Pawn pawn = null;
			if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
			{
				pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
				num = 0.6f;
			}
			else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
			{
				pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
				num = 0.1f;
			}
			else if (recipient.GetSpouseCount(includeDead: false) > 0)
			{
				pawn = recipient.GetMostLikedSpouseRelation().otherPawn;
				num = 0.3f;
			}
			if (pawn != null)
			{
				num *= Mathf.InverseLerp(100f, 0f, recipient.relations.OpinionOf(pawn));
				num *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
			}
		}
		return num;
	}

	private static float OpinionFactor(Pawn initiator, Pawn recipient)
	{
		return Mathf.InverseLerp(5f, 100f, recipient.relations.OpinionOf(initiator));
	}

	public static string RomanceFactors(Pawn romancer, Pawn romanceTarget)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(RomanceFactorLine("RomanceChanceOpinionFactor".Translate(), OpinionFactor(romancer, romanceTarget)));
		stringBuilder.AppendLine(RomanceFactorLine("RomanceChanceAgeFactor".Translate(), romanceTarget.relations.LovinAgeFactor(romancer)));
		float num = PartnerFactor(romancer, romanceTarget);
		if (num != 1f)
		{
			stringBuilder.AppendLine(RomanceFactorLine("RomanceChancePartnerFactor".Translate(), num));
		}
		float num2 = romanceTarget.relations.PrettinessFactor(romancer);
		if (num2 != 1f)
		{
			stringBuilder.AppendLine(RomanceFactorLine("RomanceChanceBeautyFactor".Translate(), num2));
		}
		HediffWithTarget hediffWithTarget = (HediffWithTarget)romanceTarget.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicLove);
		if (hediffWithTarget != null && hediffWithTarget.target == romancer)
		{
			stringBuilder.AppendLine(RomanceFactorLine("PsychicLoveFor".Translate() + " " + romancer.LabelShort, 10f));
		}
		if (ModsConfig.BiotechActive)
		{
			if (romancer.genes != null)
			{
				List<Gene> genesListForReading = romancer.genes.GenesListForReading;
				for (int i = 0; i < genesListForReading.Count; i++)
				{
					if (genesListForReading[i].Active && genesListForReading[i].def.missingGeneRomanceChanceFactor != 1f && (romanceTarget.genes == null || !romanceTarget.genes.HasActiveGene(genesListForReading[i].def)))
					{
						float value = genesListForReading[i].def.missingGeneRomanceChanceFactor;
						string text = string.Empty;
						if (romanceTarget.story?.traits != null && romanceTarget.story.traits.HasTrait(TraitDefOf.Kind))
						{
							value = 1f;
							text = " (" + TraitDefOf.Kind.DataAtDegree(0).label + ")";
						}
						stringBuilder.AppendLine(RomanceFactorLine(genesListForReading[i].def.LabelCap + " (" + romancer.NameShortColored.Resolve() + ")", value) + text);
					}
				}
			}
			if (romanceTarget.genes != null)
			{
				List<Gene> genesListForReading2 = romanceTarget.genes.GenesListForReading;
				for (int j = 0; j < genesListForReading2.Count; j++)
				{
					if (genesListForReading2[j].Active && genesListForReading2[j].def.missingGeneRomanceChanceFactor != 1f && (romancer.genes == null || !romancer.genes.HasActiveGene(genesListForReading2[j].def)))
					{
						float value2 = genesListForReading2[j].def.missingGeneRomanceChanceFactor;
						string text2 = string.Empty;
						if (romanceTarget.story?.traits != null && romancer.story.traits.HasTrait(TraitDefOf.Kind))
						{
							value2 = 1f;
							text2 = " (" + TraitDefOf.Kind.DataAtDegree(0).label + ")";
						}
						stringBuilder.AppendLine(RomanceFactorLine(genesListForReading2[j].def.LabelCap + " (" + romanceTarget.NameShortColored + ")", value2) + text2);
					}
				}
			}
		}
		return stringBuilder.ToString();
	}

	private static string RomanceFactorLine(string label, float value)
	{
		return " - " + label + ": x" + value.ToStringPercent();
	}

	public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
	{
		if (initiator.CurJob?.def == JobDefOf.TryRomance)
		{
			initiator.relations.romanceEnableTick = Find.TickManager.TicksGame + 900000;
		}
		if (Rand.Value < SuccessChance(initiator, recipient))
		{
			BreakLoverAndFianceRelations(initiator, out var oldLoversAndFiances);
			BreakLoverAndFianceRelations(recipient, out var oldLoversAndFiances2);
			RemoveBrokeUpAndFailedRomanceThoughts(initiator, recipient);
			RemoveBrokeUpAndFailedRomanceThoughts(recipient, initiator);
			for (int i = 0; i < oldLoversAndFiances.Count; i++)
			{
				TryAddCheaterThought(oldLoversAndFiances[i], initiator);
			}
			for (int j = 0; j < oldLoversAndFiances2.Count; j++)
			{
				TryAddCheaterThought(oldLoversAndFiances2[j], recipient);
			}
			initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExLover, recipient);
			initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
			TaleRecorder.RecordTale(TaleDefOf.BecameLover, initiator, recipient);
			bool createdBond = false;
			if (CanCreatePsychicBondBetween(initiator, recipient))
			{
				createdBond = TryCreatePsychicBondBetween(initiator, recipient);
			}
			if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
			{
				GetNewLoversLetter(initiator, recipient, oldLoversAndFiances, oldLoversAndFiances2, createdBond, out letterText, out letterLabel, out letterDef, out lookTargets);
			}
			else
			{
				letterText = null;
				letterLabel = null;
				letterDef = null;
				lookTargets = null;
			}
			extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptAccepted);
			LovePartnerRelationUtility.TryToShareBed(initiator, recipient);
		}
		else
		{
			if (initiator.needs.mood != null)
			{
				initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
			}
			if (recipient.needs.mood != null)
			{
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
			}
			if (recipient.needs.mood != null && recipient.relations.OpinionOf(initiator) <= 0)
			{
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
			}
			extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptRejected);
			letterText = null;
			letterLabel = null;
			letterDef = null;
			lookTargets = null;
			if (initiator.CurJob?.def == JobDefOf.TryRomance)
			{
				Messages.Message("TryRomanceFailedMessage".Translate(initiator, recipient), initiator, MessageTypeDefOf.NegativeEvent, historical: false);
			}
		}
	}

	public static bool CanCreatePsychicBondBetween(Pawn initiator, Pawn recipient)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		Gene_PsychicBonding gene_PsychicBonding = initiator.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
		Gene_PsychicBonding gene_PsychicBonding2 = recipient.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
		if (gene_PsychicBonding == null && gene_PsychicBonding2 == null)
		{
			return false;
		}
		if (gene_PsychicBonding == null || gene_PsychicBonding.CanBondToNewPawn)
		{
			return gene_PsychicBonding2?.CanBondToNewPawn ?? true;
		}
		return false;
	}

	public static bool TryCreatePsychicBondBetween(Pawn initiator, Pawn recipient)
	{
		Gene_PsychicBonding gene_PsychicBonding = initiator.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
		Gene_PsychicBonding gene_PsychicBonding2 = recipient.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
		if (gene_PsychicBonding != null && gene_PsychicBonding2 != null && (!gene_PsychicBonding.CanBondToNewPawn || !gene_PsychicBonding2.CanBondToNewPawn))
		{
			return false;
		}
		if (gene_PsychicBonding != null && gene_PsychicBonding.CanBondToNewPawn)
		{
			gene_PsychicBonding.BondTo(recipient);
			return true;
		}
		if (gene_PsychicBonding2 != null && gene_PsychicBonding2.CanBondToNewPawn)
		{
			gene_PsychicBonding2.BondTo(initiator);
			return true;
		}
		return false;
	}

	private void RemoveBrokeUpAndFailedRomanceThoughts(Pawn pawn, Pawn otherPawn)
	{
		if (pawn.needs.mood != null)
		{
			pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, otherPawn);
			pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, otherPawn);
			pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, otherPawn);
		}
	}

	private void BreakLoverAndFianceRelations(Pawn pawn, out List<Pawn> oldLoversAndFiances)
	{
		oldLoversAndFiances = new List<Pawn>();
		int num = 200;
		while (num > 0 && !new HistoryEvent(pawn.GetHistoryEventForLoveRelationCountPlusOne(), pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			Pawn pawn2 = LovePartnerRelationUtility.ExistingLeastLikedPawnWithRelation(pawn, (DirectPawnRelation r) => r.def == PawnRelationDefOf.Lover);
			if (pawn2 != null)
			{
				pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, pawn2);
				pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, pawn2);
				oldLoversAndFiances.Add(pawn2);
				num--;
				continue;
			}
			Pawn pawn3 = LovePartnerRelationUtility.ExistingLeastLikedPawnWithRelation(pawn, (DirectPawnRelation r) => r.def == PawnRelationDefOf.Fiance);
			if (pawn3 != null)
			{
				pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, pawn3);
				pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, pawn3);
				oldLoversAndFiances.Add(pawn3);
				num--;
				continue;
			}
			break;
		}
	}

	private void TryAddCheaterThought(Pawn pawn, Pawn cheater)
	{
		if (!pawn.Dead && pawn.needs.mood != null)
		{
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, cheater);
		}
	}

	private void GetNewLoversLetter(Pawn initiator, Pawn recipient, List<Pawn> initiatorOldLoversAndFiances, List<Pawn> recipientOldLoversAndFiances, bool createdBond, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
	{
		bool flag = false;
		HistoryEvent ev = new HistoryEvent(initiator.GetHistoryEventLoveRelationCount(), initiator.Named(HistoryEventArgsNames.Doer));
		HistoryEvent ev2 = new HistoryEvent(recipient.GetHistoryEventLoveRelationCount(), recipient.Named(HistoryEventArgsNames.Doer));
		if (!ev.DoerWillingToDo() || !ev2.DoerWillingToDo())
		{
			letterLabel = "LetterLabelAffair".Translate();
			letterDef = LetterDefOf.NegativeEvent;
			flag = true;
		}
		else
		{
			letterLabel = "LetterLabelNewLovers".Translate();
			letterDef = LetterDefOf.PositiveEvent;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (BedUtility.WillingToShareBed(initiator, recipient))
		{
			stringBuilder.AppendLineTagged("LetterNewLovers".Translate(initiator.Named("PAWN1"), recipient.Named("PAWN2")));
		}
		if (flag)
		{
			Pawn firstSpouse = initiator.GetFirstSpouse();
			if (firstSpouse != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLineTagged("LetterAffair".Translate(initiator.LabelShort, firstSpouse.LabelShort, recipient.LabelShort, initiator.Named("PAWN1"), recipient.Named("PAWN2"), firstSpouse.Named("SPOUSE")));
			}
			Pawn firstSpouse2 = recipient.GetFirstSpouse();
			if (firstSpouse2 != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLineTagged("LetterAffair".Translate(recipient.LabelShort, firstSpouse2.LabelShort, initiator.LabelShort, recipient.Named("PAWN1"), firstSpouse2.Named("SPOUSE"), initiator.Named("PAWN2")));
			}
		}
		for (int i = 0; i < initiatorOldLoversAndFiances.Count; i++)
		{
			if (!initiatorOldLoversAndFiances[i].Dead)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLineTagged("LetterNoLongerLovers".Translate(initiator.LabelShort, initiatorOldLoversAndFiances[i].LabelShort, initiator.Named("PAWN1"), initiatorOldLoversAndFiances[i].Named("PAWN2")));
			}
		}
		for (int j = 0; j < recipientOldLoversAndFiances.Count; j++)
		{
			if (!recipientOldLoversAndFiances[j].Dead)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLineTagged("LetterNoLongerLovers".Translate(recipient.LabelShort, recipientOldLoversAndFiances[j].LabelShort, recipient.Named("PAWN1"), recipientOldLoversAndFiances[j].Named("PAWN2")));
			}
		}
		if (createdBond)
		{
			Pawn pawn = ((initiator.genes.GetFirstGeneOfType<Gene_PsychicBonding>() != null) ? initiator : recipient);
			Pawn arg = ((pawn == initiator) ? recipient : initiator);
			stringBuilder.AppendLine();
			stringBuilder.AppendLineTagged("LetterPsychicBondCreated".Translate(pawn.Named("BONDPAWN"), arg.Named("OTHERPAWN")));
		}
		letterText = stringBuilder.ToString().TrimEndNewlines();
		lookTargets = new LookTargets(initiator, recipient);
	}
}
