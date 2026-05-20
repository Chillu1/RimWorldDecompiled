using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Pawn_InteractionsTracker : IExposable
{
	private Pawn pawn;

	private bool wantsRandomInteract;

	private int lastInteractionTime = -9999;

	private string lastInteraction;

	private InteractionDef lastInteractionDef;

	private int lastOccultInteraction = -9999;

	private int lastRambling = -9999;

	private const int RandomInteractMTBTicks_Quiet = 22000;

	private const int RandomInteractMTBTicks_Normal = 6600;

	private const int RandomInteractMTBTicks_SuperActive = 550;

	public const int RandomInteractIntervalMin = 320;

	private const int RandomInteractCheckInterval = 60;

	private const float SlaveSocialFightFactor = 0.5f;

	private const int ChildSocialFightAgeRange = 6;

	private const int InteractIntervalAbsoluteMin = 120;

	public const int DirectTalkInteractInterval = 320;

	public const float IdeoExposurePointsInteraction = 0.5f;

	private const float MetalhorrorDetectionChance = 0.001f;

	private const float MetalhorrorDetectionNoticed = 0.3f;

	private const float DisturbingInhumanRamblingDaysMTB = 1f;

	private const float OccultInteractionWait = 60000f;

	private List<Pawn> tmpPawns;

	private List<int> tmpTicks;

	private static readonly List<Pawn> workingList = new List<Pawn>();

	public InteractionDef LastInteractionDef => lastInteractionDef;

	private RandomSocialMode CurrentSocialMode
	{
		get
		{
			if (!SocialInteractionUtility.CanInitiateRandomInteraction(pawn))
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
			if (duty != null && duty.SocialModeMax < randomSocialMode)
			{
				randomSocialMode = duty.SocialModeMax;
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
		Scribe_Values.Look(ref lastInteraction, "lastInteraction");
		Scribe_Values.Look(ref lastInteractionTime, "lastInteractionTime", -9999);
		Scribe_Values.Look(ref lastOccultInteraction, "lastOccultInteraction", -9999);
		Scribe_Values.Look(ref lastRambling, "lastRambling", -9999);
		Scribe_Defs.Look(ref lastInteractionDef, "lastInteractionDef");
	}

	public void InteractionsTrackerTickInterval(int delta)
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
			if (Find.TickManager.TicksGame > lastInteractionTime + 320 && pawn.IsHashIntervalTick(60, delta))
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
		else if (pawn.IsHashIntervalTick(91, delta) && TryInteractRandomly())
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
		if (InteractedTooRecentlyToInteract())
		{
			return false;
		}
		if (!pawn.IsCarryingPawn(recipient))
		{
			if (!recipient.Spawned)
			{
				return false;
			}
			if (!SocialInteractionUtility.IsGoodPositionForInteraction(pawn, recipient))
			{
				return false;
			}
		}
		if (!SocialInteractionUtility.CanInitiateInteraction(pawn, interactionDef) || !SocialInteractionUtility.CanReceiveInteraction(recipient, interactionDef))
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
			Log.Warning(pawn?.ToString() + " tried to interact with self, interaction=" + intDef.defName);
			return false;
		}
		if (!CanInteractNowWith(recipient, intDef))
		{
			return false;
		}
		if (!intDef.ignoreTimeSinceLastInteraction && InteractedTooRecentlyToInteract())
		{
			Log.Error($"{pawn} tried to do interaction {intDef} to {recipient} only {Find.TickManager.TicksGame - lastInteractionTime} ticks since last interaction {lastInteraction.ToStringSafe()} (min is {120}).");
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
		recipient.ideo?.IncreaseIdeoExposureIfBaby(pawn.Ideo, 0.5f);
		bool flag = false;
		if (recipient.RaceProps.Humanlike && recipient.Spawned)
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
		MoteMaker.MakeInteractionBubble(pawn, recipient, intDef.interactionMote, intDef.GetSymbol(pawn.Faction, pawn.Ideo), intDef.GetSymbolColor(pawn.Faction));
		lastInteractionTime = Find.TickManager.TicksGame;
		lastInteraction = intDef.defName;
		lastInteractionDef = intDef;
		if (recipient.interactions != null)
		{
			recipient.interactions.lastInteractionTime = Find.TickManager.TicksGame;
			recipient.interactions.lastInteraction = intDef.defName;
			recipient.interactions.lastInteractionDef = intDef;
		}
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
		if (ModsConfig.AnomalyActive)
		{
			if (intDef == InteractionDefOf.OccultTeaching)
			{
				Find.ResearchManager.ApplyKnowledge(KnowledgeCategoryDefOf.Basic, 1f);
				lastOccultInteraction = GenTicks.TicksAbs;
			}
			MetalhorrorDetectionCheck(recipient);
		}
		return true;
	}

	private void MetalhorrorDetectionCheck(Pawn recipient)
	{
		bool num = MetalhorrorUtility.IsInfected(pawn);
		Hediff_MetalhorrorImplant firstHediff = recipient.health.hediffSet.GetFirstHediff<Hediff_MetalhorrorImplant>();
		if (!num && firstHediff != null && !firstHediff.Visible && (Rand.Chance(0.001f) || firstHediff.debugDiscoverNextInteraction))
		{
			TaggedString taggedString = "MetalhorrorReasonInteraction".Translate(pawn.Named("PAWN"), recipient.Named("INFECTED"));
			TaggedString taggedString2 = "MetalhorrorNoticedDetailsAppended".Translate(pawn.Named("PAWN"), recipient.Named("INFECTED"));
			TaggedString taggedString3 = "InteractionDetectedDesc".Translate(pawn.Named("PAWN"), recipient.Named("INFECTED"));
			taggedString3 += $"\n\n{taggedString2}";
			MetalhorrorUtility.Detect(recipient, taggedString, taggedString3, 0.3f);
		}
	}

	public static void AddInteractionThought(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef)
	{
		if (pawn.needs.mood != null)
		{
			float statValue = otherPawn.GetStatValue(StatDefOf.SocialImpact);
			Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
			thought_Memory.moodPowerFactor = statValue;
			if (thought_Memory is Thought_MemorySocial thought_MemorySocial)
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
		if (!SocialInteractionUtility.CanInitiateRandomInteraction(pawn))
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
			if (p == pawn || !CanInteractNowWith(p) || !SocialInteractionUtility.CanReceiveRandomInteraction(p) || pawn.HostileTo(p))
			{
				continue;
			}
			InteractionDef result = null;
			if (ModsConfig.AnomalyActive)
			{
				if (pawn.story.traits.HasTrait(TraitDefOf.Occultist) && (float)GenTicks.TicksAbs >= (float)lastOccultInteraction + 60000f)
				{
					result = InteractionDefOf.OccultTeaching;
				}
				else if (pawn.story.IsDisturbing)
				{
					if (Rand.MTBEventOccurs(1f, 60000f, GenTicks.TicksGame - lastRambling))
					{
						MentalState_InhumanRambling.DoInhumanRambling(pawn);
						lastRambling = GenTicks.TicksGame;
						workingList.Clear();
						return true;
					}
					result = InteractionDefOf.DisturbingChat;
				}
			}
			if ((result == null || CanInteractNowWith(p, result)) && (result != null || allDefsListForReading.TryRandomElementByWeight((InteractionDef x) => (!CanInteractNowWith(p, x)) ? 0f : x.Worker.RandomSelectionWeight(pawn, p), out result)))
			{
				if (TryInteractWith(p, result))
				{
					workingList.Clear();
					return true;
				}
				Log.Error(pawn?.ToString() + " failed to interact with " + p);
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

	public void StartSocialFight(Pawn otherPawn, string messageKey = "MessageSocialFight")
	{
		if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(otherPawn))
		{
			Messages.Message(messageKey.Translate(pawn.LabelShort, otherPawn.LabelShort, pawn.Named("PAWN1"), otherPawn.Named("PAWN2")), pawn, MessageTypeDefOf.ThreatSmall);
		}
		pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, null, forced: false, forceWake: false, causedByMood: false, otherPawn);
		otherPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, null, forced: false, forceWake: false, causedByMood: false, pawn);
		TaleRecorder.RecordTale(TaleDefOf.SocialFight, pawn, otherPawn);
	}

	public bool SocialFightPossible(Pawn otherPawn)
	{
		if (!pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike)
		{
			return false;
		}
		if (!SocialInteractionUtility.HasAnyVerbForSocialFight(pawn) || !SocialInteractionUtility.HasAnyVerbForSocialFight(otherPawn))
		{
			return false;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return false;
		}
		if (otherPawn.Downed || pawn.Downed)
		{
			return false;
		}
		if (pawn.IsPrisoner && !otherPawn.IsPrisoner)
		{
			return false;
		}
		if (pawn.IsSlave && !otherPawn.IsSlave)
		{
			return false;
		}
		DevelopmentalStage developmentalStage = pawn.ageTracker.CurLifeStage.developmentalStage;
		if (developmentalStage == DevelopmentalStage.Baby)
		{
			return false;
		}
		if (Mathf.Abs(pawn.ageTracker.AgeBiologicalYears - otherPawn.ageTracker.AgeBiologicalYears) > 6 && developmentalStage == DevelopmentalStage.Child)
		{
			return false;
		}
		if (developmentalStage == DevelopmentalStage.Adult && otherPawn.ageTracker.AgeBiologicalYears < 13)
		{
			return false;
		}
		if (pawn.genes != null && pawn.genes.SocialFightChanceFactor <= 0f)
		{
			return false;
		}
		if (otherPawn.genes != null && otherPawn.genes.SocialFightChanceFactor <= 0f)
		{
			return false;
		}
		return true;
	}

	public float SocialFightChance(InteractionDef interaction, Pawn initiator)
	{
		if (!SocialFightPossible(initiator))
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
				if (!allTraits[j].Suppressed)
				{
					socialFightBaseChance *= allTraits[j].CurrentData.socialFightChanceFactor;
				}
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
		if (pawn.IsSlave)
		{
			socialFightBaseChance *= 0.5f;
		}
		if (pawn.genes != null)
		{
			socialFightBaseChance *= pawn.genes.SocialFightChanceFactor;
		}
		if (initiator.genes != null)
		{
			socialFightBaseChance *= initiator.genes.SocialFightChanceFactor;
		}
		return Mathf.Clamp01(socialFightBaseChance);
	}
}
