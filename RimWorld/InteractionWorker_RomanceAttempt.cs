using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_RomanceAttempt : InteractionWorker
	{
		private const float MinRomanceChanceForRomanceAttempt = 0.15f;

		private const int MinOpinionForRomanceAttempt = 5;

		private const float BaseSelectionWeight = 1.15f;

		private const float BaseSuccessChance = 0.6f;

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (TutorSystem.TutorialMode)
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
			int num2 = initiator.relations.OpinionOf(recipient);
			if (num2 < 5)
			{
				return 0f;
			}
			if (recipient.relations.OpinionOf(initiator) < 5)
			{
				return 0f;
			}
			float num3 = 1f;
			Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, allowDead: false);
			if (pawn != null)
			{
				float value = initiator.relations.OpinionOf(pawn);
				num3 = Mathf.InverseLerp(50f, -50f, value);
			}
			float num4 = (initiator.story.traits.HasTrait(TraitDefOf.Gay) ? 1f : ((initiator.gender == Gender.Female) ? 0.15f : 1f));
			float num5 = Mathf.InverseLerp(0.15f, 1f, num);
			float num6 = Mathf.InverseLerp(5f, 100f, num2);
			float num7 = ((initiator.gender == recipient.gender) ? ((!initiator.story.traits.HasTrait(TraitDefOf.Gay) || !recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f) : ((initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 0.15f : 1f));
			return 1.15f * num4 * num5 * num6 * num3 * num7;
		}

		public float SuccessChance(Pawn initiator, Pawn recipient)
		{
			float num = 0.6f * recipient.relations.SecondaryRomanceChanceFactor(initiator) * Mathf.InverseLerp(5f, 100f, recipient.relations.OpinionOf(initiator));
			float num2 = 1f;
			Pawn pawn = null;
			if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
			{
				pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
				num2 = 0.6f;
			}
			else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
			{
				pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
				num2 = 0.1f;
			}
			else if (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead)
			{
				pawn = recipient.GetSpouse();
				num2 = 0.3f;
			}
			if (pawn != null)
			{
				num2 *= Mathf.InverseLerp(100f, 0f, recipient.relations.OpinionOf(pawn));
				num2 *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
			}
			return Mathf.Clamp01(num * num2);
		}

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			if (Rand.Value < SuccessChance(initiator, recipient))
			{
				BreakLoverAndFianceRelations(initiator, out var oldLoversAndFiances);
				BreakLoverAndFianceRelations(recipient, out var oldLoversAndFiances2);
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
				if (initiator.needs.mood != null)
				{
					initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, recipient);
					initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, recipient);
					initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, recipient);
				}
				if (recipient.needs.mood != null)
				{
					recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, initiator);
					recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
					recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
				}
				if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
				{
					GetNewLoversLetter(initiator, recipient, oldLoversAndFiances, oldLoversAndFiances2, out letterText, out letterLabel, out letterDef, out lookTargets);
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
			}
		}

		private void BreakLoverAndFianceRelations(Pawn pawn, out List<Pawn> oldLoversAndFiances)
		{
			oldLoversAndFiances = new List<Pawn>();
			while (true)
			{
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
				if (firstDirectRelationPawn != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
					oldLoversAndFiances.Add(firstDirectRelationPawn);
					continue;
				}
				Pawn firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
				if (firstDirectRelationPawn2 != null)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn2);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn2);
					oldLoversAndFiances.Add(firstDirectRelationPawn2);
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

		private void GetNewLoversLetter(Pawn initiator, Pawn recipient, List<Pawn> initiatorOldLoversAndFiances, List<Pawn> recipientOldLoversAndFiances, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			bool flag = false;
			if ((initiator.GetSpouse() != null && !initiator.GetSpouse().Dead) || (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead))
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
			stringBuilder.AppendLine("LetterNewLovers".Translate(initiator.Named("PAWN1"), recipient.Named("PAWN2")));
			stringBuilder.AppendLine();
			if (flag)
			{
				if (initiator.GetSpouse() != null)
				{
					stringBuilder.AppendLine("LetterAffair".Translate(initiator.LabelShort, initiator.GetSpouse().LabelShort, recipient.LabelShort, initiator.Named("PAWN1"), recipient.Named("PAWN2"), initiator.GetSpouse().Named("SPOUSE")));
				}
				if (recipient.GetSpouse() != null)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("LetterAffair".Translate(recipient.LabelShort, recipient.GetSpouse().LabelShort, initiator.LabelShort, recipient.Named("PAWN1"), recipient.GetSpouse().Named("SPOUSE"), initiator.Named("PAWN2")));
				}
			}
			for (int i = 0; i < initiatorOldLoversAndFiances.Count; i++)
			{
				if (!initiatorOldLoversAndFiances[i].Dead)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("LetterNoLongerLovers".Translate(initiator.LabelShort, initiatorOldLoversAndFiances[i].LabelShort, initiator.Named("PAWN1"), initiatorOldLoversAndFiances[i].Named("PAWN2")));
				}
			}
			for (int j = 0; j < recipientOldLoversAndFiances.Count; j++)
			{
				if (!recipientOldLoversAndFiances[j].Dead)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("LetterNoLongerLovers".Translate(recipient.LabelShort, recipientOldLoversAndFiances[j].LabelShort, recipient.Named("PAWN1"), recipientOldLoversAndFiances[j].Named("PAWN2")));
				}
			}
			letterText = stringBuilder.ToString().TrimEndNewlines();
			lookTargets = new LookTargets(initiator, recipient);
		}
	}
}
