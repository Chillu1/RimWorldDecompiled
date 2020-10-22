using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_RecruitAttempt : InteractionWorker
	{
		private const float BaseResistanceReductionPerInteraction = 1f;

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

		public const float WildmanWildness = 0.75f;

		private const float WildmanPrisonerChanceFactor = 0.6f;

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
			float x = ((recipient.relations != null) ? recipient.relations.OpinionOf(initiator) : 0);
			bool flag2 = initiator.InspirationDef == InspirationDefOf.Inspired_Recruitment && !flag && recipient.guest.interactionMode != PrisonerInteractionModeDefOf.ReduceResistance;
			if (DebugSettings.instantRecruit)
			{
				recipient.guest.resistance = 0f;
			}
			float resistanceReduce = 0f;
			if (!flag && recipient.guest.resistance > 0f && !flag2)
			{
				float num = 1f;
				num *= initiator.GetStatValue(StatDefOf.NegotiationAbility);
				num *= ResistanceImpactFactorCurve_Mood.Evaluate((recipient.needs.mood == null) ? 1f : recipient.needs.mood.CurInstantLevelPercentage);
				num *= ResistanceImpactFactorCurve_Opinion.Evaluate(x);
				num = Mathf.Min(num, recipient.guest.resistance);
				float resistance = recipient.guest.resistance;
				recipient.guest.resistance = Mathf.Max(0f, recipient.guest.resistance - num);
				resistanceReduce = resistance - recipient.guest.resistance;
				string text = "TextMote_ResistanceReduced".Translate(resistance.ToString("F1"), recipient.guest.resistance.ToString("F1"));
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
					TaggedString taggedString = "MessagePrisonerResistanceBroken".Translate(recipient.LabelShort, initiator.LabelShort, initiator.Named("WARDEN"), recipient.Named("PRISONER"));
					if (recipient.guest.interactionMode == PrisonerInteractionModeDefOf.AttemptRecruit)
					{
						taggedString += " " + "MessagePrisonerResistanceBroken_RecruitAttempsWillBegin".Translate();
					}
					Messages.Message(taggedString, recipient, MessageTypeDefOf.PositiveEvent);
				}
			}
			else
			{
				float num2;
				if (!flag)
				{
					num2 = ((!flag2 && !DebugSettings.instantRecruit) ? recipient.RecruitChanceFinalByPawn(initiator) : 1f);
				}
				else if (initiator.InspirationDef == InspirationDefOf.Inspired_Taming)
				{
					num2 = 1f;
					initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Taming);
				}
				else
				{
					num2 = initiator.GetStatValue(StatDefOf.TameAnimalChance);
					float x2 = (recipient.IsWildMan() ? 0.75f : recipient.RaceProps.wildness);
					num2 *= TameChanceFactorCurve_Wildness.Evaluate(x2);
					if (recipient.IsPrisonerInPrisonCell())
					{
						num2 *= 0.6f;
					}
					if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
					{
						num2 *= 4f;
					}
				}
				if (Rand.Chance(num2))
				{
					if (!flag)
					{
						recipient.guest.ClearLastRecruiterData();
					}
					DoRecruit(initiator, recipient, num2, out letterLabel, out letterText, useAudiovisualEffects: true, sendLetter: false);
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
				}
				else
				{
					string text2 = (flag ? "TextMote_TameFail".Translate(num2.ToStringPercent()) : "TextMote_RecruitFail".Translate(num2.ToStringPercent()));
					if (!flag)
					{
						if (recipient.needs.mood != null && recipient.needs.mood.CurLevelPercentage < 0.4f)
						{
							text2 += "\n(" + "lowMood".Translate() + ")";
						}
						if (recipient.relations != null && (float)recipient.relations.OpinionOf(initiator) < -0.01f)
						{
							text2 += "\n(" + "lowOpinion".Translate() + ")";
						}
					}
					MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text2, 8f);
					recipient.mindState.CheckStartMentalStateBecauseRecruitAttempted(initiator);
					extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
				}
			}
			if (!flag)
			{
				recipient.guest.SetLastRecruiterData(initiator, resistanceReduce);
			}
		}

		public static void DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
		{
			DoRecruit(recruiter, recruitee, recruitChance, out var _, out var _, useAudiovisualEffects);
		}

		public static void DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, out string letterLabel, out string letter, bool useAudiovisualEffects = true, bool sendLetter = true)
		{
			letterLabel = null;
			letter = null;
			recruitChance = Mathf.Clamp01(recruitChance);
			string value = recruitee.LabelIndefinite();
			if (recruitee.apparel != null && recruitee.apparel.LockedApparel != null)
			{
				List<Apparel> lockedApparel = recruitee.apparel.LockedApparel;
				for (int num = lockedApparel.Count - 1; num >= 0; num--)
				{
					recruitee.apparel.Unlock(lockedApparel[num]);
				}
			}
			if (recruitee.royalty != null)
			{
				foreach (RoyalTitle item in recruitee.royalty.AllTitlesForReading)
				{
					if (item.def.replaceOnRecruited != null)
					{
						recruitee.royalty.SetTitle(item.faction, item.def.replaceOnRecruited, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
					}
				}
			}
			if (recruitee.guest != null)
			{
				recruitee.guest.SetGuestStatus(null);
			}
			bool flag = recruitee.Name != null;
			if (recruitee.Faction != recruiter.Faction)
			{
				recruitee.SetFaction(recruiter.Faction, recruiter);
			}
			if (recruitee.RaceProps.Humanlike)
			{
				if (useAudiovisualEffects)
				{
					letterLabel = "LetterLabelMessageRecruitSuccess".Translate() + ": " + recruitee.LabelShortCap;
					if (sendLetter)
					{
						Find.LetterStack.ReceiveLetter(letterLabel, "MessageRecruitSuccess".Translate(recruiter, recruitee, recruitChance.ToStringPercent(), recruiter.Named("RECRUITER"), recruitee.Named("RECRUITEE")), LetterDefOf.PositiveEvent, recruitee);
					}
				}
				TaleRecorder.RecordTale(TaleDefOf.Recruited, recruiter, recruitee);
				recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
				if (recruitee.needs.mood != null)
				{
					recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
				}
				QuestUtility.SendQuestTargetSignals(recruitee.questTags, "Recruited", recruitee.Named("SUBJECT"));
			}
			else
			{
				if (useAudiovisualEffects)
				{
					if (!flag)
					{
						Messages.Message("MessageTameAndNameSuccess".Translate(recruiter.LabelShort, value, recruitChance.ToStringPercent(), recruitee.Name.ToStringFull, recruiter.Named("RECRUITER"), recruitee.Named("RECRUITEE")).AdjustedFor(recruitee), recruitee, MessageTypeDefOf.PositiveEvent);
					}
					else
					{
						Messages.Message("MessageTameSuccess".Translate(recruiter.LabelShort, value, recruitChance.ToStringPercent(), recruiter.Named("RECRUITER")), recruitee, MessageTypeDefOf.PositiveEvent);
					}
					if (recruiter.Spawned && recruitee.Spawned)
					{
						MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map, "TextMote_TameSuccess".Translate(recruitChance.ToStringPercent()), 8f);
					}
				}
				recruiter.records.Increment(RecordDefOf.AnimalsTamed);
				RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
				if (Rand.Chance(Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness)) || recruitee.IsWildMan())
				{
					TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, recruiter, recruitee);
				}
				if (PawnsFinder.AllMapsWorldAndTemporary_Alive.Count((Pawn p) => p.playerSettings != null && p.playerSettings.Master == recruiter) >= 5)
				{
					TaleRecorder.RecordTale(TaleDefOf.IncreasedMenagerie, recruiter, recruitee);
				}
			}
			if (recruitee.caller != null)
			{
				recruitee.caller.DoCall();
			}
		}
	}
}
