using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_Hospitality_Refugee : QuestNode
	{
		private static FloatRange LodgerCountBasedOnColonyPopulationFactorRange = new FloatRange(0.3f, 1f);

		private static int MaxLodgerCount = 10;

		private const float MidEventSelWeight_None = 0.5f;

		private const float MidEventSelWeight_Mutiny = 0.25f;

		private const float MidEventSelWeight_BetrayalOffer = 0.25f;

		private const float RewardPostLeaveChance = 0.5f;

		private const float RewardFactor_Postleave = 55f;

		private const float RewardFactor_BetrayalOffer = 300f;

		private const int BetrayalOfferGoodwillReward = 10;

		private static FloatRange BetrayalOfferTimeRange = new FloatRange(0.25f, 0.5f);

		private static FloatRange MutinyTimeRange = new FloatRange(0.2f, 1f);

		private static IntRange QuestDurationDaysRange = new IntRange(5, 20);

		protected override void RunInt()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Hospitality refugee is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 8811221);
				return;
			}
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = QuestGen_Get.GetMap();
			int num = (slate.Exists("population") ? slate.Get("population", 0) : map.mapPawns.FreeColonistsSpawnedCount);
			int lodgerCount = Mathf.Max(Mathf.RoundToInt(LodgerCountBasedOnColonyPopulationFactorRange.RandomInRange * (float)num), 1);
			lodgerCount = Mathf.Min(lodgerCount, MaxLodgerCount);
			int questDurationDays = QuestDurationDaysRange.RandomInRange;
			int questDurationTicks = questDurationDays * 60000;
			List<FactionRelation> list = new List<FactionRelation>();
			foreach (Faction item4 in Find.FactionManager.AllFactionsListForReading)
			{
				if (!item4.def.permanentEnemy)
				{
					list.Add(new FactionRelation
					{
						other = item4,
						goodwill = 0,
						kind = FactionRelationKind.Neutral
					});
				}
			}
			Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDefOf.OutlanderRefugee, list);
			faction.hidden = true;
			faction.temporary = true;
			faction.hostileFromMemberCapture = false;
			Find.FactionManager.Add(faction);
			string lodgerRecruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
			string text = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrested");
			string lodgerDestroyedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Destroyed");
			string lodgerKidnapped = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Kidnapped");
			string lodgerLeftMap = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftMap");
			string lodgerBanished = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Banished");
			List<Pawn> pawns = new List<Pawn>();
			for (int i = 0; i < lodgerCount; i++)
			{
				Pawn pawn = quest.GeneratePawn(PawnKindDefOf.Refugee, faction, allowAddictions: true, null, 0f, mustBeCapableOfViolence: true, null, 0f, 0f, ensureNonNumericName: false, forceGenerateNewPawn: true);
				pawns.Add(pawn);
				quest.PawnJoinOffer(pawn, "LetterJoinOfferLabel".Translate(pawn.Named("PAWN")), "LetterJoinOfferTitle".Translate(pawn.Named("PAWN")), "LetterJoinOfferText".Translate(pawn.Named("PAWN"), map.Parent.Named("MAP")), delegate
				{
					quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap, text: "MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")));
					quest.SignalPass(null, null, lodgerRecruitedSignal);
				});
			}
			slate.Set("lodgers", pawns);
			faction.leader = pawns.First();
			Pawn var = pawns.First();
			quest.SetFactionHidden(faction);
			QuestPart_ExtraFaction extraFactionPart = quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, lodgerRecruitedSignal);
			quest.PawnsArrive(pawns, null, map.Parent, null, joinPlayer: true, null, "[lodgersArriveLetterLabel]", "[lodgersArriveLetterText]");
			QuestPart_Choice questPart_Choice = quest.RewardChoice();
			QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
			{
				rewards = 
				{
					(Reward)new Reward_VisitorsHelp(),
					(Reward)new Reward_PossibleFutureReward()
				}
			};
			questPart_Choice.choices.Add(item);
			bool mutiny = false;
			string assaultColonySignal = QuestGen.GenerateNewSignal("AssaultColony");
			Action item2 = delegate
			{
				int num4 = Mathf.FloorToInt(MutinyTimeRange.RandomInRange * (float)questDurationTicks);
				quest.Delay(num4, delegate
				{
					quest.Letter(LetterDefOf.ThreatBig, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[mutinyLetterText]", null, "[mutinyLetterLabel]");
					quest.SignalPass(null, null, assaultColonySignal);
					QuestGen_End.End(quest, QuestEndOutcome.Unknown);
				}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, "Mutiny (" + num4.ToStringTicksToDays() + ")");
				mutiny = true;
			};
			Action item3 = delegate
			{
				Pawn factionOpponent = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
				{
					mustBeWorldPawn = true,
					mustBeFactionLeader = true,
					canGeneratePawn = true,
					mustBeNonHostileToPlayer = true
				});
				slate.Set("factionOpponent", factionOpponent);
				int num2 = Mathf.FloorToInt(BetrayalOfferTimeRange.RandomInRange * (float)questDurationTicks);
				quest.Delay(num2, delegate
				{
					float val2 = (float)lodgerCount * 300f;
					FloatRange value = new FloatRange(0.7f, 1.3f) * val2 * Find.Storyteller.difficultyValues.EffectiveQuestRewardValueFactor;
					ThingSetMakerParams parms = default(ThingSetMakerParams);
					parms.totalMarketValueRange = value;
					parms.qualityGenerator = QualityGenerator.Reward;
					parms.makingFaction = faction;
					List<Thing> betrayalRewardThings = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
					quest.BetrayalOffer(pawns, extraFactionPart.extraFaction, factionOpponent, delegate
					{
						float num3 = 0f;
						for (int j = 0; j < betrayalRewardThings.Count; j++)
						{
							num3 += betrayalRewardThings[j].MarketValue * (float)betrayalRewardThings[j].stackCount;
						}
						slate.Set("betrayalRewards", GenLabel.ThingsLabel(betrayalRewardThings));
						slate.Set("betrayalRewardMarketValue", num3);
						quest.DropPods(map.Parent, betrayalRewardThings, null, null, null, null, true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.Always, null, destroyItemsOnCleanup: false);
						quest.FactionGoodwillChange(factionOpponent.Faction, 10, null, canSendMessage: true, canSendHostilityLetter: true, "GoodwillChangeReason_AttackedFaction".Translate(faction), getLookTargetFromSignal: true, QuestPart.SignalListenMode.Always);
						quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.Always, betrayalRewardThings, filterDeadPawnsFromLookTargets: false, "[betrayalOfferRewardLetterText]", null, "[betrayalOfferRewardLetterLabel]");
					}, delegate
					{
						quest.DestroyThingsOrPassToWorld(betrayalRewardThings, null, questLookTargets: true, QuestPart.SignalListenMode.Always);
						quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.Always, null, filterDeadPawnsFromLookTargets: false, "[betrayalOfferFailedLetterText]", null, "[betrayalOfferFailedLetterLabel]");
					}, delegate
					{
						(quest.Letter(LetterDefOf.BetrayVisitors, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[betrayalOffserLetterText]", null, "[betrayalOfferLetterLabel]").letter as ChoiceLetter_BetrayVisitors).pawns.AddRange(pawns);
					}, new List<string>
					{
						lodgerDestroyedSignal,
						lodgerKidnapped,
						lodgerLeftMap,
						lodgerBanished
					}, null, QuestPart.SignalListenMode.Always);
				}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, "BetrayalOffer (" + num2.ToStringTicksToDays() + ")");
			};
			if (new List<Tuple<float, Action>>
			{
				Tuple.Create(0.25f, item2),
				Tuple.Create(0.25f, item3),
				Tuple.Create<float, Action>(0.5f, delegate
				{
				})
			}.TryRandomElementByWeight((Tuple<float, Action> t) => t.Item1, out var result))
			{
				result.Item2();
			}
			QuestPart_RefugeeInteractions questPart_RefugeeInteractions = new QuestPart_RefugeeInteractions();
			questPart_RefugeeInteractions.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
			questPart_RefugeeInteractions.inSignalDestroyed = lodgerDestroyedSignal;
			questPart_RefugeeInteractions.inSignalArrested = text;
			questPart_RefugeeInteractions.inSignalSurgeryViolation = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.SurgeryViolation");
			questPart_RefugeeInteractions.inSignalKidnapped = lodgerKidnapped;
			questPart_RefugeeInteractions.inSignalRecruited = lodgerRecruitedSignal;
			questPart_RefugeeInteractions.inSignalAssaultColony = assaultColonySignal;
			questPart_RefugeeInteractions.inSignalLeftMap = lodgerLeftMap;
			questPart_RefugeeInteractions.inSignalBanished = lodgerBanished;
			questPart_RefugeeInteractions.outSignalDestroyed_AssaultColony = QuestGen.GenerateNewSignal("LodgerDestroyed_AssaultColony");
			questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony = QuestGen.GenerateNewSignal("LodgerDestroyed_LeaveColony");
			questPart_RefugeeInteractions.outSignalDestroyed_BadThought = QuestGen.GenerateNewSignal("LodgerDestroyed_BadThought");
			questPart_RefugeeInteractions.outSignalArrested_AssaultColony = QuestGen.GenerateNewSignal("LodgerArrested_AssaultColony");
			questPart_RefugeeInteractions.outSignalArrested_LeaveColony = QuestGen.GenerateNewSignal("LodgerArrested_LeaveColony");
			questPart_RefugeeInteractions.outSignalArrested_BadThought = QuestGen.GenerateNewSignal("LodgerArrested_BadThought");
			questPart_RefugeeInteractions.outSignalSurgeryViolation_AssaultColony = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_AssaultColony");
			questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_LeaveColony");
			questPart_RefugeeInteractions.outSignalSurgeryViolation_BadThought = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_BadThought");
			questPart_RefugeeInteractions.outSignalLast_Destroyed = QuestGen.GenerateNewSignal("LastLodger_Destroyed");
			questPart_RefugeeInteractions.outSignalLast_Arrested = QuestGen.GenerateNewSignal("LastLodger_Arrested");
			questPart_RefugeeInteractions.outSignalLast_Kidnapped = QuestGen.GenerateNewSignal("LastLodger_Kidnapped");
			questPart_RefugeeInteractions.outSignalLast_Recruited = QuestGen.GenerateNewSignal("LastLodger_Recruited");
			questPart_RefugeeInteractions.outSignalLast_LeftMapAllHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllHealthy");
			questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllNotHealthy");
			questPart_RefugeeInteractions.outSignalLast_Banished = QuestGen.GenerateNewSignal("LastLodger_Banished");
			questPart_RefugeeInteractions.pawns.AddRange(pawns);
			questPart_RefugeeInteractions.faction = faction;
			questPart_RefugeeInteractions.mapParent = map.Parent;
			questPart_RefugeeInteractions.signalListenMode = QuestPart.SignalListenMode.Always;
			quest.AddPart(questPart_RefugeeInteractions);
			string lodgerArrestedOrRecruited = QuestGen.GenerateNewSignal("Lodger_ArrestedOrRecruited");
			quest.AnySignal(new List<string>
			{
				lodgerRecruitedSignal,
				text
			}, null, new List<string>
			{
				lodgerArrestedOrRecruited
			});
			if (!mutiny)
			{
				quest.Delay(questDurationTicks, delegate
				{
					quest.SignalPassWithFaction(faction, null, delegate
					{
						quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersLeavingLetterText]", null, "[lodgersLeavingLetterLabel]");
					});
					quest.Leave(pawns, null, sendStandardLetter: false, leaveOnCleanup: false, lodgerArrestedOrRecruited);
				}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, "GuestsDepartsIn".Translate(), "GuestsDepartsOn".Translate(), "QuestDelay");
			}
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalDestroyed_BadThought, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedMemoryThoughtLetterText]", null, "[lodgerDiedMemoryThoughtLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalDestroyed_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedAttackPlayerLetterText]", null, "[lodgerDiedAttackPlayerLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedLeaveMapLetterText]", null, "[lodgerDiedLeaveMapLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalLast_Destroyed, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllDiedLetterText]", null, "[lodgersAllDiedLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalArrested_BadThought, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedMemoryThoughtLetterText]", null, "[lodgerArrestedMemoryThoughtLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalArrested_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedAttackPlayerLetterText]", null, "[lodgerArrestedAttackPlayerLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalArrested_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedLeaveMapLetterText]", null, "[lodgerArrestedLeaveMapLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalLast_Arrested, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllArrestedLetterText]", null, "[lodgersAllArrestedLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalSurgeryViolation_BadThought, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedMemoryThoughtLetterText]", null, "[lodgerViolatedMemoryThoughtLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalSurgeryViolation_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedAttackPlayerLetterText]", null, "[lodgerViolatedAttackPlayerLetterLabel]");
			quest.Letter(LetterDefOf.NegativeEvent, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedLeaveMapLetterText]", null, "[lodgerViolatedLeaveMapLetterLabel]");
			quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_RefugeeInteractions.outSignalDestroyed_BadThought);
			quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_RefugeeInteractions.outSignalArrested_BadThought);
			quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_RefugeeInteractions.outSignalSurgeryViolation_BadThought);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalDestroyed_AssaultColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalDestroyed_LeaveColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalLast_Destroyed);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalArrested_AssaultColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalArrested_LeaveColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalLast_Arrested);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalSurgeryViolation_AssaultColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalSurgeryViolation_LeaveColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalLast_Kidnapped, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_RefugeeInteractions.outSignalLast_Banished, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Success, 0, null, questPart_RefugeeInteractions.outSignalLast_Recruited, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Success, 0, null, questPart_RefugeeInteractions.outSignalLast_LeftMapAllNotHealthy, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.SignalPass(delegate
			{
				if (Rand.Chance(0.5f))
				{
					float val = (float)(lodgerCount * questDurationDays) * 55f;
					FloatRange marketValueRange = new FloatRange(0.7f, 1.3f) * val * Find.Storyteller.difficultyValues.EffectiveQuestRewardValueFactor;
					quest.AddQuestRefugeeDelayedReward(quest.AccepterPawn, faction, pawns, marketValueRange);
				}
				quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			}, questPart_RefugeeInteractions.outSignalLast_LeftMapAllHealthy);
			slate.Set("lodgerCount", lodgerCount);
			slate.Set("lodgersCountMinusOne", lodgerCount - 1);
			slate.Set("asker", var);
			slate.Set("map", map);
			slate.Set("questDurationTicks", questDurationTicks);
			slate.Set("faction", faction);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return QuestGen_Get.GetMap() != null;
		}
	}
}
