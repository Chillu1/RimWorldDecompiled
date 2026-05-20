using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Beggars : QuestNode
{
	private static readonly FloatRange LodgerCountBasedOnColonyPopulationFactorRange_Low = new FloatRange(0.3f, 1f);

	private static readonly FloatRange LodgerCountBaseOnColonyPopulationFactorRange_High = new FloatRange(0.25f, 0.75f);

	private static readonly IntRange PopulationCountToCurve = new IntRange(5, 15);

	private const int VisitDuration = 60000;

	private const float BeggarRequestValueFactor = 0.85f;

	private const float MaxRequestValue = 700f;

	private const float GiveQuestChance = 0.35f;

	private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();

	private static IEnumerable<ThingDef> AllowedThings
	{
		get
		{
			yield return ThingDefOf.Silver;
			yield return ThingDefOf.MedicineHerbal;
			yield return ThingDefOf.MedicineIndustrial;
			yield return ThingDefOf.Penoxycyline;
			yield return ThingDefOf.Beer;
		}
	}

	protected override void RunInt()
	{
		if (!ModLister.CheckIdeology("Beggars"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		float points = slate.Get("points", 0f);
		slate.Set("visitDurationTicks", 60000);
		float totalRequestValue = GetTotalRequestValue(points);
		if (TryFindRandomRequestedThing(map, totalRequestValue, out var thingDef, out var count, AllowedThings))
		{
			slate.Set("requestedThing", thingDef);
			slate.Set("requestedThingDefName", thingDef.defName);
			slate.Set("requestedThingCount", count);
		}
		int num = LodgerCountFromPopulation(slate, map);
		List<FactionRelation> list = new List<FactionRelation>();
		foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
		{
			if (!item.def.PermanentlyHostileTo(FactionDefOf.Beggars))
			{
				list.Add(new FactionRelation
				{
					other = item,
					kind = FactionRelationKind.Neutral
				});
			}
		}
		Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDefOf.Beggars, list, hidden: true);
		faction.temporary = true;
		Find.FactionManager.Add(faction);
		slate.Set("faction", faction);
		slate.Set("map", map);
		int num2 = 0;
		bool var = false;
		if (Find.Storyteller.difficulty.ChildrenAllowed)
		{
			new List<(int, float)>
			{
				(0, 0.4f),
				(Rand.Range(1, Mathf.Max(1, num / 2)), 0.4f),
				(num, 0.2f)
			}.TryRandomElementByWeight(((int, float) p) => p.Item2, out var result);
			num2 = result.Item1;
			var = num == num2;
		}
		slate.Set("childCount", num2);
		slate.Set("allChildren", var);
		List<Pawn> pawns = new List<Pawn>();
		for (int num3 = 0; num3 < num; num3++)
		{
			DevelopmentalStage developmentalStage = ((num3 >= num - num2) ? DevelopmentalStage.Child : DevelopmentalStage.Adult);
			List<Pawn> list2 = pawns;
			Quest quest2 = quest;
			PawnKindDef beggar = PawnKindDefOf.Beggar;
			Faction faction2 = faction;
			DevelopmentalStage developmentalStages = developmentalStage;
			list2.Add(quest2.GeneratePawn(new PawnGenerationRequest(beggar, faction2, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, developmentalStages)));
		}
		for (int num4 = 0; num4 < pawns.Count; num4++)
		{
			Pawn pawn = pawns[num4];
			if (pawn.inventory == null)
			{
				continue;
			}
			for (int num5 = pawn.inventory.innerContainer.Count - 1; num5 >= 0; num5--)
			{
				if (pawn.inventory.innerContainer[num5].def == thingDef)
				{
					pawn.inventory.innerContainer[num5].Destroy();
				}
			}
		}
		slate.Set("beggars", pawns);
		slate.Set("beggarCount", num);
		quest.SetFactionHidden(faction);
		quest.PawnsArrive(pawns, null, map.Parent, null, joinPlayer: false, null, null, null, null, null, isSingleReward: false, rewardDetailsHidden: false, sendStandardLetter: false);
		string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("Receiver");
		string itemsReceivedSignal = QuestGen.GenerateNewSignal("ItemsReceived");
		Pawn pawn2 = pawns[0];
		QuestUtility.AddQuestTag(ref pawn2.questTags, text);
		QuestPart_BegForItems questPart_BegForItems = new QuestPart_BegForItems();
		questPart_BegForItems.inSignal = QuestGen.slate.Get<string>("inSignal");
		questPart_BegForItems.outSignalItemsReceived = itemsReceivedSignal;
		questPart_BegForItems.pawns.AddRange(pawns);
		questPart_BegForItems.target = pawn2;
		questPart_BegForItems.faction = faction;
		questPart_BegForItems.mapParent = map.Parent;
		questPart_BegForItems.thingDef = thingDef;
		questPart_BegForItems.amount = count;
		quest.AddPart(questPart_BegForItems);
		quest.GiveRewards(new RewardsGeneratorParams
		{
			rewardValue = totalRequestValue,
			allowDevelopmentPoints = true,
			thingRewardDisallowed = true
		});
		if (ModsConfig.OdysseyActive)
		{
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID(text + ".ReceivedItems");
			List<QuestScriptDef> list3 = QuestUtility.GetGiverQuests(QuestGiverTag.Beggars).ToList();
			if (Rand.Chance(0.35f) && !list3.NullOrEmpty())
			{
				QuestPart_AddGiverQuest questPart_AddGiverQuest = new QuestPart_AddGiverQuest();
				questPart_AddGiverQuest.inSignal = inSignal;
				questPart_AddGiverQuest.questScript = list3.RandomElementByWeight((QuestScriptDef q) => q.rootSelectionWeight);
				questPart_AddGiverQuest.discoveryMethodTranslationKey = "QuestDiscoveredFromBeggar";
				questPart_AddGiverQuest.sendAvailableLetter = true;
				questPart_AddGiverQuest.points = points;
				quest.AddPart(questPart_AddGiverQuest);
			}
		}
		string pawnLabelSingleOrPlural = ((num > 1) ? faction.def.pawnsPlural : faction.def.pawnSingular);
		quest.Delay(60000, delegate
		{
			quest.Leave(pawns, null, sendStandardLetter: false, leaveOnCleanup: false);
			quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_Beggars);
			quest.AnyColonistWithCharityPrecept(delegate
			{
				quest.Message(string.Format("{0}: {1}", "MessageCharityEventRefused".Translate(), "MessageBeggarsLeavingWithNoItems".Translate(pawnLabelSingleOrPlural)), MessageTypeDefOf.NegativeEvent, getLookTargetsFromSignal: false, null, pawns);
			});
		}, null, itemsReceivedSignal);
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID("beggars.Arrested");
		string text3 = QuestGenUtility.HardcodedSignalWithQuestID("beggars.Killed");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("beggars.LeftMap");
		TaggedString leavingWithItemsMessage = ((num > 1) ? "MessageBeggarsLeavingWithItemsPlural".Translate(pawnLabelSingleOrPlural) : "MessageBeggarsLeavingWithItemsSingular".Translate(pawnLabelSingleOrPlural));
		quest.AnyColonistWithCharityPrecept(delegate
		{
			quest.Message("MessageCharityEventFulfilled".Translate() + ": " + leavingWithItemsMessage, MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, pawns);
			quest.RecordHistoryEvent(HistoryEventDefOf.CharityFulfilled_Beggars);
		}, delegate
		{
			quest.Message(leavingWithItemsMessage, MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, pawns);
		}, itemsReceivedSignal);
		quest.AnySignal(new string[2] { text3, text2 }, delegate
		{
			quest.SignalPassActivable(delegate
			{
				quest.AnyColonistWithCharityPrecept(delegate
				{
					quest.Message(string.Format("{0}: {1}", "MessageCharityEventRefused".Translate(), "MessageBeggarsLeavingWithNoItems".Translate(pawnLabelSingleOrPlural)), MessageTypeDefOf.NegativeEvent, getLookTargetsFromSignal: false, null, pawns);
				});
			}, null, null, null, null, itemsReceivedSignal);
			quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, pawns, filterDeadPawnsFromLookTargets: false, "[letterTextBeggarsBetrayed]", null, "[letterLabelBeggarsBetrayed]");
			QuestPart_FactionRelationChange part = new QuestPart_FactionRelationChange
			{
				faction = faction,
				relationKind = FactionRelationKind.Hostile,
				canSendHostilityLetter = false,
				inSignal = QuestGen.slate.Get<string>("inSignal")
			};
			quest.AddPart(part);
			quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_Beggars_Betrayed);
		});
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved"));
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("faction.BecameHostileToPlayer"));
		quest.AllPawnsDespawned(pawns, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Success);
		}, null, inSignal2);
	}

	public static int LodgerCountFromPopulation(Slate slate, Map map)
	{
		int num = (slate.Exists("population") ? slate.Get("population", 0) : map.mapPawns.FreeAdultColonistsSpawnedCount);
		float num2 = 1f;
		if (num <= PopulationCountToCurve.TrueMin)
		{
			num2 = LodgerCountBasedOnColonyPopulationFactorRange_Low.RandomInRange;
		}
		else if (num >= PopulationCountToCurve.TrueMax)
		{
			num2 = LodgerCountBaseOnColonyPopulationFactorRange_High.RandomInRange;
		}
		else
		{
			float t = Mathf.InverseLerp(PopulationCountToCurve.TrueMin, PopulationCountToCurve.TrueMax, num);
			FloatRange floatRange = new FloatRange(Mathf.Lerp(LodgerCountBasedOnColonyPopulationFactorRange_Low.TrueMin, LodgerCountBaseOnColonyPopulationFactorRange_High.TrueMin, t), Mathf.Lerp(LodgerCountBasedOnColonyPopulationFactorRange_Low.TrueMax, LodgerCountBaseOnColonyPopulationFactorRange_High.TrueMax, t));
			num2 = floatRange.RandomInRange;
		}
		return Mathf.Max(Mathf.RoundToInt((float)num * num2), 1);
	}

	private static float GetTotalRequestValue(float points)
	{
		return Mathf.Min(points * 0.85f, 700f);
	}

	private static bool TryFindRandomRequestedThing(Map map, float value, out ThingDef thingDef, out int count, IEnumerable<ThingDef> allowedThings)
	{
		requestCountDict.Clear();
		Func<ThingDef, bool> globalValidator = delegate(ThingDef td)
		{
			if (!td.PlayerAcquirable)
			{
				return false;
			}
			int num = ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(value / td.BaseMarketValue)));
			requestCountDict.Add(td, num);
			return PlayerItemAccessibilityUtility.Accessible(td, num, map) ? true : false;
		};
		if (allowedThings.Where((ThingDef def) => globalValidator(def)).TryRandomElement(out thingDef))
		{
			count = requestCountDict[thingDef];
			return true;
		}
		count = 0;
		return false;
	}

	protected override bool TestRunInt(Slate slate)
	{
		Map map = QuestGen_Get.GetMap();
		if (map == null)
		{
			return false;
		}
		if (!FactionDefOf.Beggars.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp))
		{
			return false;
		}
		float points = slate.Get("points", 0f);
		ThingDef thingDef;
		int count;
		return TryFindRandomRequestedThing(map, GetTotalRequestValue(points), out thingDef, out count, AllowedThings);
	}

	[DebugOutput]
	public static void BeggarQuestItems()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Possible items with quantities based on points using the current map:");
		for (int i = 0; i < 10000; i += 100)
		{
			stringBuilder.AppendLine($"========== points {i} ==========");
			foreach (ThingDef allowedThing in AllowedThings)
			{
				if (TryFindRandomRequestedThing(Find.CurrentMap, GetTotalRequestValue(i), out var thingDef, out var count, Gen.YieldSingle(allowedThing)))
				{
					stringBuilder.AppendLine($"<color=green>[POSSIBLE]</color> {thingDef.label} x{count}");
				}
				else
				{
					stringBuilder.AppendLine("<color=red>[NOT POSSIBLE]</color> " + allowedThing.label);
				}
			}
		}
		Log.Message(stringBuilder.ToString());
	}
}
