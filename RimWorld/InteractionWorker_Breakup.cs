using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class InteractionWorker_Breakup : InteractionWorker
{
	private const float BaseChance = 0.02f;

	private const float SpouseRelationChanceFactor = 0.4f;

	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
		{
			return 0f;
		}
		float num = Mathf.InverseLerp(100f, -100f, initiator.relations.OpinionOf(recipient));
		float num2 = 1f;
		if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
		{
			num2 = 0.4f;
		}
		float num3 = 1f;
		HediffWithTarget hediffWithTarget = (HediffWithTarget)initiator.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicLove);
		if (hediffWithTarget != null && hediffWithTarget.target == recipient)
		{
			num3 = 0.1f;
		}
		return 0.02f * num * num2 * num3;
	}

	public Thought RandomBreakupReason(Pawn initiator, Pawn recipient)
	{
		if (initiator.needs.mood == null)
		{
			return null;
		}
		List<Thought_Memory> list = initiator.needs.mood.thoughts.memories.Memories.Where((Thought_Memory m) => m != null && m.otherPawn == recipient && m.CurStage != null && m.CurStage.baseOpinionOffset < 0f).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		float worstMemoryOpinionOffset = list.Max((Thought_Memory m) => 0f - m.CurStage.baseOpinionOffset);
		Thought_Memory result = null;
		list.Where((Thought_Memory m) => 0f - m.CurStage.baseOpinionOffset >= worstMemoryOpinionOffset / 2f).TryRandomElementByWeight((Thought_Memory m) => 0f - m.CurStage.baseOpinionOffset, out result);
		return result;
	}

	public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
	{
		Thought thought = RandomBreakupReason(initiator, recipient);
		bool flag = false;
		bool flag2 = false;
		if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
		{
			initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
			initiator.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, recipient);
			SpouseRelationUtility.RemoveGotMarriedThoughts(initiator, recipient);
			flag = SpouseRelationUtility.ChangeNameAfterDivorce(initiator);
			flag2 = SpouseRelationUtility.ChangeNameAfterDivorce(recipient);
		}
		else
		{
			initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
			initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, recipient);
			initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
			if (recipient.needs.mood != null)
			{
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BrokeUpWithMe, initiator);
			}
		}
		if (initiator.ownership.OwnedBed != null && initiator.ownership.OwnedBed == recipient.ownership.OwnedBed)
		{
			((Rand.Value < 0.5f) ? initiator : recipient).ownership.UnclaimBed();
		}
		TaleRecorder.RecordTale(TaleDefOf.Breakup, initiator, recipient);
		if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("LetterNoLongerLovers".Translate(initiator.LabelShort, recipient.LabelShort, initiator.Named("PAWN1"), recipient.Named("PAWN2")));
			stringBuilder.AppendLine();
			if (flag)
			{
				stringBuilder.Append("LetterNoLongerLovers_BackToBirthName".Translate(initiator.Named("PAWN")));
			}
			if (flag2)
			{
				if (flag)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append("LetterNoLongerLovers_BackToBirthName".Translate(recipient.Named("PAWN")));
			}
			if (flag || flag2)
			{
				stringBuilder.AppendLine();
			}
			if (thought != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("FinalStraw".Translate(thought.LabelCap));
			}
			letterLabel = "LetterLabelBreakup".Translate();
			letterText = stringBuilder.ToString().TrimEndNewlines();
			letterDef = LetterDefOf.NegativeEvent;
			lookTargets = new LookTargets(initiator, recipient);
		}
		else
		{
			letterLabel = null;
			letterText = null;
			letterDef = null;
			lookTargets = null;
		}
	}
}
