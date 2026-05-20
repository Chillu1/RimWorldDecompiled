using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_ReliquaryPilgrims : QuestNode
{
	private static IntRange VisitDurationTicksRange = new IntRange(5000, 10000);

	private static IntRange PilgrimCount = new IntRange(1, 4);

	private static float RewardChance = 0.5f;

	private static FloatRange RewardMarketValueRange = new FloatRange(1000f, 2000f);

	private static IntRange RewardDelayRangeTicks = new IntRange(300000, 600000);

	private const string RootSymbol = "root";

	protected override void RunInt()
	{
		if (!ModLister.CheckIdeology("Reliquary pilgrims"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		int randomInRange = PilgrimCount.RandomInRange;
		if (map == null)
		{
			return;
		}
		GetFactionAndPawnKind(out var factionDef, out var pawnKind);
		TryFindReliquaryWithRelic(map, out var relic, out var reliquary, out var relicThing);
		List<FactionRelation> list = new List<FactionRelation>();
		foreach (Faction item4 in Find.FactionManager.AllFactionsListForReading)
		{
			if (!item4.def.PermanentlyHostileTo(factionDef))
			{
				list.Add(new FactionRelation
				{
					other = item4,
					kind = FactionRelationKind.Neutral
				});
			}
		}
		Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(factionDef, list, hidden: true);
		faction.temporary = true;
		Find.FactionManager.Add(faction);
		List<Pawn> list2 = new List<Pawn>();
		int num = 0;
		bool var = false;
		if (Find.Storyteller.difficulty.ChildrenAllowed)
		{
			new List<(int, float)>
			{
				(0, 0.25f),
				(Rand.Range(1, Mathf.Max(1, randomInRange / 2)), 0.5f),
				(randomInRange, 0.25f)
			}.TryRandomElementByWeight(((int, float) p) => p.Item2, out var result);
			num = result.Item1;
			var = randomInRange == num;
		}
		slate.Set("childCount", num);
		slate.Set("allChildren", var);
		for (int num2 = 0; num2 < randomInRange; num2++)
		{
			DevelopmentalStage developmentalStages = ((num2 >= randomInRange - num) ? DevelopmentalStage.Child : DevelopmentalStage.Adult);
			Pawn pawn = quest.GeneratePawn(pawnKind, faction, allowAddictions: true, null, 0f, mustBeCapableOfViolence: true, null, 0f, 0f, ensureNonNumericName: false, forceGenerateNewPawn: true, developmentalStages);
			pawn.ideo.SetIdeo(relic.ideo);
			list2.Add(pawn);
		}
		quest.SetFactionHidden(faction);
		quest.PawnsArrive(list2, null, map.Parent, null, joinPlayer: false, null, "[pilgrimsArrivedLetterLabel]", "[pilgrimsArrivedLetterText]");
		string text = QuestGen.GenerateNewSignal("VenerationCompleted");
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID("relicThing.Despawned");
		QuestPart_Venerate questPart_Venerate = new QuestPart_Venerate();
		questPart_Venerate.inSignal = QuestGen.slate.Get<string>("inSignal");
		questPart_Venerate.inSignalForceExit = text2;
		questPart_Venerate.pawns.AddRange(list2);
		questPart_Venerate.target = reliquary;
		questPart_Venerate.venerateDurationTicks = VisitDurationTicksRange.RandomInRange;
		questPart_Venerate.faction = faction;
		questPart_Venerate.mapParent = map.Parent;
		questPart_Venerate.outSignalVenerationCompleted = text;
		quest.AddPart(questPart_Venerate);
		Quest quest2 = quest;
		MessageTypeDef neutralEvent = MessageTypeDefOf.NeutralEvent;
		string inSignal = text;
		quest2.Message("[pilgrimsLeavingMessage]", neutralEvent, getLookTargetsFromSignal: false, null, list2, inSignal);
		string text3 = QuestGen.GenerateNewSignal("AllLeftMap");
		QuestPart_PassAll questPart_PassAll = new QuestPart_PassAll();
		questPart_PassAll.outSignal = text3;
		quest.AddPart(questPart_PassAll);
		slate.Set("pawns", list2);
		slate.Set("faction", faction);
		foreach (Pawn item5 in list2)
		{
			string text4 = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(item5.ThingID);
			QuestUtility.AddQuestTag(item5, text4);
			string item = QuestGenUtility.HardcodedSignalWithQuestID(text4 + ".LeftMap");
			questPart_PassAll.inSignals.Add(item);
		}
		ThingSetMakerParams parms = new ThingSetMakerParams
		{
			totalMarketValueRange = RewardMarketValueRange,
			qualityGenerator = QualityGenerator.Reward,
			makingFaction = faction,
			countRange = new IntRange(1, 1)
		};
		List<Thing> list3 = ThingSetMakerDefOf.Reward_ReliquaryPilgrims.root.Generate(parms);
		QuestPart_DelayedRewardDropPods delayedReward = new QuestPart_DelayedRewardDropPods();
		delayedReward.inSignal = text3;
		delayedReward.faction = faction;
		delayedReward.giver = list2[0];
		delayedReward.rewards.AddRange(list3);
		delayedReward.delayTicks = RewardDelayRangeTicks.RandomInRange;
		delayedReward.chance = RewardChance;
		QuestGen.AddTextRequest("root", delegate(string x)
		{
			delayedReward.customLetterText = x;
		}, QuestGenUtility.MergeRules(null, "[delayedRewardLetterText]", "root"));
		quest.Message("[pilgrimsLeftMessage]", MessageTypeDefOf.NeutralEvent, getLookTargetsFromSignal: false, null, null, text3);
		quest.AddPart(delayedReward);
		string item2 = QuestGenUtility.HardcodedSignalWithQuestID("pawns.Arrested");
		string item3 = QuestGenUtility.HardcodedSignalWithQuestID("pawns.Killed");
		quest.AnySignal(new List<string> { item2, item3 }, delegate
		{
			quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_Pilgrims_Betrayed, null, QuestPart.SignalListenMode.Always);
			QuestPart_FactionRelationChange part = new QuestPart_FactionRelationChange
			{
				faction = faction,
				relationKind = FactionRelationKind.Hostile,
				canSendHostilityLetter = false,
				inSignal = QuestGen.slate.Get<string>("inSignal")
			};
			quest.AddPart(part);
		});
		string text5 = QuestGen.GenerateNewSignal("RelicInvalidated");
		QuestPart_PassAnyActivable questPart_PassAnyActivable = new QuestPart_PassAnyActivable();
		questPart_PassAnyActivable.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
		questPart_PassAnyActivable.inSignals.Add(text2);
		questPart_PassAnyActivable.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID("reliquary.Destroyed"));
		questPart_PassAnyActivable.outSignalsCompleted.Add(text5);
		questPart_PassAnyActivable.inSignalDisable = text;
		quest.AddPart(questPart_PassAnyActivable);
		quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_Pilgrims, text2);
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)new Reward_PossibleFutureReward() }
		};
		if (Faction.OfPlayer.ideos.FluidIdeo != null)
		{
			choice.rewards.Add(new Reward_DevelopmentPoints(quest));
		}
		questPart_Choice.choices.Add(choice);
		quest.End(QuestEndOutcome.Fail, 0, null, text5, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved"), QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("faction.BecameHostileToPlayer"), QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Success, 0, null, text3);
		slate.Set("map", map);
		slate.Set("relic", relic);
		slate.Set("relicThing", relicThing);
		slate.Set("reliquary", reliquary);
		slate.Set("pilgrimCount", randomInRange);
		slate.Set("rewards", GenLabel.ThingsLabel(list3));
		slate.Set("rewardsMarketValue", TradeUtility.TotalMarketValue(list3));
		slate.Set("venerateDate", GenDate.DateFullStringAt(GenDate.TickGameToAbs(quest.acceptanceTick), Find.WorldGrid.LongLatOf(map.Tile)));
		slate.Set("pilgrimFaction", faction.def == FactionDefOf.Pilgrims);
	}

	private static bool TryFindReliquaryWithRelic(Map map, out Precept_Relic relic, out Building reliquary, out Thing relicThing)
	{
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.Reliquary).InRandomOrder())
		{
			CompThingContainer compThingContainer = item.TryGetComp<CompThingContainer>();
			if (compThingContainer == null)
			{
				continue;
			}
			foreach (Thing item2 in (IEnumerable<Thing>)compThingContainer.GetDirectlyHeldThings())
			{
				if (item2.StyleSourcePrecept is Precept_Relic precept_Relic)
				{
					reliquary = (Building)item;
					relic = precept_Relic;
					relicThing = item2;
					return true;
				}
			}
		}
		reliquary = null;
		relic = null;
		relicThing = null;
		return false;
	}

	private void GetFactionAndPawnKind(out FactionDef factionDef, out PawnKindDef pawnKind)
	{
		if (Rand.Bool)
		{
			factionDef = FactionDefOf.Pilgrims;
			pawnKind = PawnKindDefOf.PovertyPilgrim;
		}
		else
		{
			factionDef = FactionDefOf.OutlanderCivil;
			pawnKind = PawnKindDefOf.WellEquippedTraveler;
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		Map map = QuestGen_Get.GetMap();
		Precept_Relic relic;
		Building reliquary;
		Thing relicThing;
		if (map != null)
		{
			return TryFindReliquaryWithRelic(map, out relic, out reliquary, out relicThing);
		}
		return false;
	}
}
