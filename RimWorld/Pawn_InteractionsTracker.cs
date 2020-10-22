using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Pawn_InteractionsTracker : IExposable
	{
		private Pawn pawn;

		private bool wantsRandomInteract;

		private int lastInteractionTime = -9999;

		private const int RandomInteractMTBTicks_Quiet = 22000;

		private const int RandomInteractMTBTicks_Normal = 6600;

		private const int RandomInteractMTBTicks_SuperActive = 550;

		public const int RandomInteractIntervalMin = 320;

		private const int RandomInteractCheckInterval = 60;

		private const int InteractIntervalAbsoluteMin = 120;

		public const int DirectTalkInteractInterval = 320;

		private static List<Pawn> workingList = new List<Pawn>();

		private RandomSocialMode CurrentSocialMode
		{
			get
			{
				if (!InteractionUtility.CanInitiateRandomInteraction(pawn))
				{
					return RandomSocialMode.Off;
				}
				RandomSocialMode randomSocialMode = RandomSocialMode.Normal;
				JobDriver curDriver = pawn.jobs.curDriver;
				if (curDriver != null)
				{
					randomSocialMode = curDriver.DesiredSocialMode();
				}
				PawnDuty duty = pawn.mindState.duty;
				if (duty != null && duty.def.socialModeMax < randomSocialMode)
				{
					randomSocialMode = duty.def.socialModeMax;
				}
				if (pawn.Drafted && randomSocialMode > RandomSocialMode.Quiet)
				{
					randomSocialMode = RandomSocialMode.Quiet;
				}
				if (pawn.InMentalState && randomSocialMode > pawn.MentalState.SocialModeMax())
				{
					randomSocialMode = pawn.MentalState.SocialModeMax();
				}
				return randomSocialMode;
			}
		}

		public Pawn_InteractionsTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref wantsRandomInteract, "wantsRandomInteract", defaultValue: false);
			Scribe_Values.Look(ref lastInteractionTime, "lastInteractionTime", -9999);
		}

		public void InteractionsTrackerTick()
		{
			RandomSocialMode currentSocialMode = CurrentSocialMode;
			switch (currentSocialMode)
			{
			case RandomSocialMode.Off:
				wantsRandomInteract = false;
				return;
			case RandomSocialMode.Quiet:
				wantsRandomInteract = false;
				break;
			}
			if (!wantsRandomInteract)
			{
				if (Find.TickManager.TicksGame > lastInteractionTime + 320 && pawn.IsHashIntervalTick(60))
				{
					int num = 0;
					switch (currentSocialMode)
					{
					case RandomSocialMode.Quiet:
						num = 22000;
						break;
					case RandomSocialMode.Normal:
						num = 6600;
						break;
					case RandomSocialMode.SuperActive:
						num = 550;
						break;
					}
					if (Rand.MTBEventOccurs(num, 1f, 60f) && !TryInteractRandomly())
					{
						wantsRandomInteract = true;
					}
				}
			}
			else if (pawn.IsHashIntervalTick(91) && TryInteractRandomly())
			{
				wantsRandomInteract = false;
			}
		}

		public bool InteractedTooRecentlyToInteract()
		{
			return Find.TickManager.TicksGame < lastInteractionTime + 120;
		}

		public bool CanInteractNowWith(Pawn recipient, InteractionDef interactionDef = null)
		{
			if (!recipient.Spawned)
			{
				return false;
			}
			if (!InteractionUtility.IsGoodPositionForInteraction(pawn, recipient))
			{
				return false;
			}
			if (!InteractionUtility.CanInitiateInteraction(pawn, interactionDef) || !InteractionUtility.CanReceiveInteraction(recipient, interactionDef))
			{
				return false;
			}
			return true;
		}

		public bool TryInteractWith(Pawn recipient, InteractionDef intDef)
		{
			if (DebugSettings.alwaysSocialFight)
			{
				intDef = InteractionDefOf.Insult;
			}
			if (pawn == recipient)
			{
				Log.Warning(string.Concat(pawn, " tried to interact with self, interaction=", intDef.defName));
				return false;
			}
			if (!CanInteractNowWith(recipient, intDef))
			{
				return false;
			}
			if (!intDef.ignoreTimeSinceLastInteraction && InteractedTooRecentlyToInteract())
			{
				Log.Error(string.Concat(pawn, " tried to do interaction ", intDef, " to ", recipient, " only ", Find.TickManager.TicksGame - lastInteractionTime, " ticks since last interaction (min is ", 120, ")."));
				return false;
			}
			List<RulePackDef> list = new List<RulePackDef>();
			if (intDef.initiatorThought != null)
			{
				AddInteractionThought(pawn, recipient, intDef.initiatorThought);
			}
			if (intDef.recipientThought != null && recipient.needs.mood != null)
			{
				AddInteractionThought(recipient, pawn, intDef.recipientThought);
			}
			if (intDef.initiatorXpGainSkill != null)
			{
				pawn.skills.Learn(intDef.initiatorXpGainSkill, intDef.initiatorXpGainAmount);
			}
			if (intDef.recipientXpGainSkill != null && recipient.RaceProps.Humanlike)
			{
				recipient.skills.Learn(intDef.recipientXpGainSkill, intDef.recipientXpGainAmount);
			}
			bool flag = false;
			if (recipient.RaceProps.Humanlike)
			{
				flag = recipient.interactions.CheckSocialFightStart(intDef, pawn);
			}
			string letterText;
			string letterLabel;
			LetterDef letterDef;
			LookTargets lookTargets;
			if (!flag)
			{
				intDef.Worker.Interacted(pawn, recipient, list, out letterText, out letterLabel, out letterDef, out lookTargets);
			}
			else
			{
				letterText = null;
				letterLabel = null;
				letterDef = null;
				lookTargets = null;
			}
			MoteMaker.MakeInteractionBubble(pawn, recipient, intDef.interactionMote, intDef.Symbol);
			lastInteractionTime = Find.TickManager.TicksGame;
			if (flag)
			{
				list.Add(RulePackDefOf.Sentence_SocialFightStarted);
			}
			PlayLogEntry_Interaction playLogEntry_Interaction = new PlayLogEntry_Interaction(intDef, pawn, recipient, list);
			Find.PlayLog.Add(playLogEntry_Interaction);
			if (letterDef != null)
			{
				string text = playLogEntry_Interaction.ToGameStringFromPOV(pawn);
				if (!letterText.NullOrEmpty())
				{
					text = text + "\n\n" + letterText;
				}
				Find.LetterStack.ReceiveLetter(letterLabel, text, letterDef, lookTargets ?? ((LookTargets)pawn));
			}
			return true;
		}

		private static void AddInteractionThought(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef)
		{
			if (pawn.needs.mood != null)
			{
				float statValue = otherPawn.GetStatValue(StatDefOf.SocialImpact);
				Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
				thought_Memory.moodPowerFactor = statValue;
				Thought_MemorySocial thought_MemorySocial = thought_Memory as Thought_MemorySocial;
				if (thought_MemorySocial != null)
				{
					thought_MemorySocial.opinionOffset *= statValue;
				}
				pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, otherPawn);
			}
		}

		private bool TryInteractRandomly()
		{
			if (InteractedTooRecentlyToInteract())
			{
				return false;
			}
			if (!InteractionUtility.CanInitiateRandomInteraction(pawn))
			{
				return false;
			}
			List<Pawn> collection = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
			workingList.Clear();
			workingList.AddRange(collection);
			workingList.Shuffle();
			List<InteractionDef> allDefsListForReading = DefDatabase<InteractionDef>.AllDefsListForReading;
			for (int i = 0; i < workingList.Count; i++)
			{
				Pawn p = workingList[i];
				if (p != pawn && CanInteractNowWith(p) && InteractionUtility.CanReceiveRandomInteraction(p) && !pawn.HostileTo(p) && allDefsListForReading.TryRandomElementByWeight((InteractionDef x) => (!CanInteractNowWith(p, x)) ? 0f : x.Worker.RandomSelectionWeight(pawn, p), out var result))
				{
					if (TryInteractWith(p, result))
					{
						workingList.Clear();
						return true;
					}
					Log.Error(string.Concat(pawn, " failed to interact with ", p));
				}
			}
			workingList.Clear();
			return false;
		}

		public bool CheckSocialFightStart(InteractionDef interaction, Pawn initiator)
		{
			if (!DebugSettings.enableRandomMentalStates)
			{
				return false;
			}
			if (pawn.needs.mood == null || TutorSystem.TutorialMode)
			{
				return false;
			}
			if (DebugSettings.alwaysSocialFight || Rand.Value < SocialFightChance(interaction, initiator))
			{
				StartSocialFight(initiator);
				return true;
			}
			return false;
		}

		public void StartSocialFight(Pawn otherPawn)
		{
			if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(otherPawn))
			{
				Messages.Message("MessageSocialFight".Translate(pawn.LabelShort, otherPawn.LabelShort, pawn.Named("PAWN1"), otherPawn.Named("PAWN2")), pawn, MessageTypeDefOf.ThreatSmall);
			}
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, null, forceWake: false, causedByMood: false, otherPawn);
			otherPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, null, forceWake: false, causedByMood: false, pawn);
			TaleRecorder.RecordTale(TaleDefOf.SocialFight, pawn, otherPawn);
		}

		public float SocialFightChance(InteractionDef interaction, Pawn initiator)
		{
			if (!pawn.RaceProps.Humanlike || !initiator.RaceProps.Humanlike)
			{
				return 0f;
			}
			if (!InteractionUtility.HasAnyVerbForSocialFight(pawn) || !InteractionUtility.HasAnyVerbForSocialFight(initiator))
			{
				return 0f;
			}
			if (pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return 0f;
			}
			if (initiator.Downed || pawn.Downed)
			{
				return 0f;
			}
			float socialFightBaseChance = interaction.socialFightBaseChance;
			socialFightBaseChance *= Mathf.InverseLerp(0.3f, 1f, pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
			socialFightBaseChance *= Mathf.InverseLerp(0.3f, 1f, pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].CurStage != null)
				{
					socialFightBaseChance *= hediffs[i].CurStage.socialFightChanceFactor;
				}
			}
			float num = pawn.relations.OpinionOf(initiator);
			socialFightBaseChance = ((!(num < 0f)) ? (socialFightBaseChance * GenMath.LerpDouble(0f, 100f, 1f, 0.6f, num)) : (socialFightBaseChance * GenMath.LerpDouble(-100f, 0f, 4f, 1f, num)));
			if (pawn.RaceProps.Humanlike)
			{
				List<Trait> allTraits = pawn.story.traits.allTraits;
				for (int j = 0; j < allTraits.Count; j++)
				{
					socialFightBaseChance *= allTraits[j].CurrentData.socialFightChanceFactor;
				}
			}
			int num2 = Mathf.Abs(pawn.ageTracker.AgeBiologicalYears - initiator.ageTracker.AgeBiologicalYears);
			if (num2 > 10)
			{
				if (num2 > 50)
				{
					num2 = 50;
				}
				socialFightBaseChance *= GenMath.LerpDouble(10f, 50f, 1f, 0.25f, num2);
			}
			return Mathf.Clamp01(socialFightBaseChance);
		}
	}
}
