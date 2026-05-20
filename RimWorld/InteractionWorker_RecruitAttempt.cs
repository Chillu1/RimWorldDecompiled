using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class InteractionWorker_RecruitAttempt : InteractionWorker
{
	public const float BaseResistanceReductionPerInteraction = 1f;

	private static readonly SimpleCurve ResistanceImpactFactorCurve_Mood = new SimpleCurve
	{
		new CurvePoint(0f, 0.2f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(1f, 1.5f)
	};

	private static readonly SimpleCurve ResistanceImpactFactorCurve_Opinion = new SimpleCurve
	{
		new CurvePoint(-100f, 0.5f),
		new CurvePoint(0f, 1f),
		new CurvePoint(100f, 1.5f)
	};

	private const float MaxMoodForWarning = 0.4f;

	private const float MaxOpinionForWarning = -0.01f;

	private const float WildmanPrisonerChanceFactor = 0.6f;

	private const float VeneratedAnimalChanceFactor = 2f;

	private static readonly SimpleCurve TameChanceFactorCurve_Wildness = new SimpleCurve
	{
		new CurvePoint(1f, 0f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(0f, 2f)
	};

	private const float TameChanceFactor_Bonded = 4f;

	private const float ChanceToDevelopBondRelationOnTamed = 0.01f;

	private const int MenagerieTaleThreshold = 5;

	public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
	{
		letterText = null;
		letterLabel = null;
		letterDef = null;
		lookTargets = null;
		bool flag = recipient.AnimalOrWildMan();
		int num = recipient.relations?.OpinionOf(initiator) ?? 0;
		bool flag2 = initiator.InspirationDef == InspirationDefOf.Inspired_Recruitment && !flag && recipient.guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.ReduceResistance);
		if (DebugSettings.instantRecruit)
		{
			recipient.guest.resistance = 0f;
		}
		if (flag)
		{
			float num2;
			if (initiator.InspirationDef == InspirationDefOf.Inspired_Taming)
			{
				num2 = 1f;
				initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Taming);
			}
			else
			{
				num2 = initiator.GetStatValue(StatDefOf.TameAnimalChance);
				float statValue = recipient.GetStatValue(StatDefOf.Wildness);
				num2 *= TameChanceFactorCurve_Wildness.Evaluate(statValue);
				if (recipient.IsPrisonerInPrisonCell())
				{
					num2 *= 0.6f;
				}
				if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
				{
					num2 *= 4f;
				}
				if (initiator.Ideo != null && initiator.Ideo.IsVeneratedAnimal(recipient))
				{
					num2 *= 2f;
				}
			}
			if (Rand.Chance(num2))
			{
				DoRecruit(initiator, recipient, out letterLabel, out letterText, useAudiovisualEffects: true, sendLetter: false);
				if (!letterLabel.NullOrEmpty())
				{
					letterDef = LetterDefOf.PositiveEvent;
				}
				lookTargets = new LookTargets(recipient, initiator);
				extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
			}
			else
			{
				TaggedString taggedString = "TextMote_TameFail".Translate(num2.ToStringPercent());
				MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, taggedString, 8f);
				recipient.mindState.CheckStartMentalStateBecauseRecruitAttempted(initiator);
				extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
			}
		}
		else if (recipient.guest.resistance > 0f && !flag2)
		{
			float num3 = ResistanceImpactFactorCurve_Mood.Evaluate((recipient.needs.mood == null) ? 1f : recipient.needs.mood.CurInstantLevelPercentage);
			float num4 = ResistanceImpactFactorCurve_Opinion.Evaluate(num);
			float statValue2 = initiator.GetStatValue(StatDefOf.NegotiationAbility);
			float num5 = 1f;
			num5 *= statValue2;
			num5 *= num3;
			num5 *= num4;
			float resistanceReduce = num5;
			num5 = Mathf.Min(num5, recipient.guest.resistance);
			float resistance = recipient.guest.resistance;
			recipient.guest.resistance = Mathf.Max(0f, recipient.guest.resistance - num5);
			_ = recipient.guest.resistance;
			if (recipient.guest.resistance <= 0f)
			{
				recipient.guest.SetLastResistanceReduceData(initiator, resistanceReduce, statValue2, num3, num4);
			}
			float num6 = ((resistance > 0f) ? Mathf.Max(0.1f, resistance) : 0f);
			float num7 = ((recipient.guest.resistance > 0f) ? Mathf.Max(0.1f, recipient.guest.resistance) : 0f);
			string text = "TextMote_ResistanceReduced".Translate(num6.ToString("F1"), num7.ToString("F1"));
			if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
			{
				text += "\n(" + "lowMood".Translate() + ")";
			}
			if (recipient.relations != null && (float)recipient.relations.OpinionOf(initiator) < -0.01f)
			{
				text += "\n(" + "lowOpinion".Translate() + ")";
			}
			MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
			if (recipient.guest.resistance == 0f)
			{
				TaggedString taggedString2 = "MessagePrisonerResistanceBroken".Translate(recipient.LabelShort, initiator.LabelShort, initiator.Named("WARDEN"), recipient.Named("PRISONER"));
				if (recipient.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.AttemptRecruit))
				{
					taggedString2 += " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
				}
				Messages.Message(taggedString2, recipient, MessageTypeDefOf.PositiveEvent);
			}
		}
		else
		{
			DoRecruit(initiator, recipient, out letterLabel, out letterText, useAudiovisualEffects: true, sendLetter: false);
			if (!letterLabel.NullOrEmpty())
			{
				letterDef = LetterDefOf.PositiveEvent;
			}
			lookTargets = new LookTargets(recipient, initiator);
			if (flag2)
			{
				initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Recruitment);
			}
			extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
			recipient.guest.SetRecruitmentData(initiator);
		}
	}

	public static void DoRecruit(Pawn recruiter, Pawn recruitee, bool useAudiovisualEffects = true)
	{
		DoRecruit(recruiter, recruitee, out var _, out var _, useAudiovisualEffects);
	}

	public static void DoRecruit(Pawn recruiter, Pawn recruitee, out string letterLabel, out string letter, bool useAudiovisualEffects = true, bool sendLetter = true)
	{
		letterLabel = null;
		letter = null;
		string text = recruitee.LabelIndefinite();
		Faction faction = recruiter?.Faction ?? Faction.OfPlayer;
		if (recruiter == null)
		{
			sendLetter = false;
			useAudiovisualEffects = false;
		}
		bool flag = recruitee.Name != null;
		RecruitUtility.Recruit(recruitee, faction, recruiter);
		if (recruitee.RaceProps.Humanlike)
		{
			if (useAudiovisualEffects)
			{
				letterLabel = "LetterLabelMessageRecruitSuccess".Translate() + ": " + recruitee.LabelShortCap;
				if (sendLetter)
				{
					Find.LetterStack.ReceiveLetter(letterLabel, "MessageRecruitSuccess".Translate(recruiter, recruitee, recruiter.Named("RECRUITER"), recruitee.Named("RECRUITEE")), LetterDefOf.PositiveEvent, recruitee);
				}
			}
			if (recruiter != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.Recruited, recruiter, recruitee);
				recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
				if (recruitee.needs.mood != null)
				{
					recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
				}
			}
			QuestUtility.SendQuestTargetSignals(recruitee.questTags, "Recruited", recruitee.Named("SUBJECT"));
		}
		else
		{
			if (useAudiovisualEffects)
			{
				if (!flag)
				{
					Messages.Message("MessageTameAndNameSuccess".Translate(recruiter.LabelShort, text, recruitee.Name.ToStringFull, recruiter.Named("RECRUITER"), recruitee.Named("RECRUITEE")).AdjustedFor(recruitee), recruitee, MessageTypeDefOf.PositiveEvent);
				}
				else
				{
					Messages.Message("MessageTameSuccess".Translate(recruiter.LabelShort, text, recruiter.Named("RECRUITER")), recruitee, MessageTypeDefOf.PositiveEvent);
				}
				if (recruiter.Spawned && recruitee.Spawned)
				{
					MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map, "TextMote_TameSuccess".Translate(), 8f);
				}
			}
			if (recruiter != null)
			{
				recruiter.records.Increment(RecordDefOf.AnimalsTamed);
				RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
				if (Rand.Chance(Mathf.Lerp(0.02f, 1f, recruitee.GetStatValue(StatDefOf.Wildness))) || recruitee.IsWildMan())
				{
					TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, recruiter, recruitee);
				}
				if (PawnsFinder.AllMapsWorldAndTemporary_Alive.Count((Pawn p) => p.playerSettings != null && p.playerSettings.Master == recruiter) >= 5)
				{
					TaleRecorder.RecordTale(TaleDefOf.IncreasedMenagerie, recruiter, recruitee);
				}
			}
		}
		if (recruitee.caller != null)
		{
			recruitee.caller.DoCall();
		}
	}
}
