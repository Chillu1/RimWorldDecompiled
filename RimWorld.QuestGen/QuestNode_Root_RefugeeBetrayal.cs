using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_RefugeeBetrayal : QuestNode
{
	public const string FailureRecruitedSignal = "BetrayalOfferFailureRecruited";

	public const string FailureLeftSignal = "BetrayalOfferFailure";

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		slate.Set("map", map);
		List<Pawn> list = slate.Get<List<Pawn>>("lodgers");
		ExtraFaction extraFaction = slate.Get<ExtraFaction>("refugeeFaction");
		Pawn factionOpponent = slate.Get<Pawn>("factionOpponent");
		float num = (float)list.Count * 300f;
		FloatRange value = new FloatRange(0.7f, 1.3f) * num * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
		ThingSetMakerParams parms = new ThingSetMakerParams
		{
			totalMarketValueRange = value,
			qualityGenerator = QualityGenerator.Reward,
			makingFaction = extraFaction.faction
		};
		string item = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
		string item2 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.BecameMutant");
		string item3 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Destroyed");
		string item4 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Kidnapped");
		string item5 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftMap");
		string item6 = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Banished");
		List<Thing> betrayalRewardThings = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
		quest.ExtraFaction(extraFaction.faction, list, ExtraFactionType.MiniFaction, areHelpers: false, new List<string> { item, item2 });
		quest.BetrayalOffer(list, extraFaction, factionOpponent, delegate
		{
			float num2 = 0f;
			for (int i = 0; i < betrayalRewardThings.Count; i++)
			{
				num2 += betrayalRewardThings[i].MarketValue * (float)betrayalRewardThings[i].stackCount;
			}
			slate.Set("betrayalRewards", GenLabel.ThingsLabel(betrayalRewardThings));
			slate.Set("betrayalRewardMarketValue", num2);
			quest.DropPods(map.Parent, betrayalRewardThings, null, null, null, null, true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.Always, null, destroyItemsOnCleanup: false, dropAllInSamePod: false, allowFogged: false, canRetargetAnyMap: true);
			quest.FactionGoodwillChange(factionOpponent.Faction, 10, null, canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, null, QuestPart.SignalListenMode.Always);
			quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.Always, betrayalRewardThings, filterDeadPawnsFromLookTargets: false, "[betrayalOfferRewardLetterText]", null, "[betrayalOfferRewardLetterLabel]");
			quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.Always);
		}, delegate
		{
			string text;
			string text2;
			if (QuestGen.slate.Get<string>("inSignal").EndsWith("BetrayalOfferFailureRecruited"))
			{
				text = "[betrayalOfferFailedBecauseRecruitedLetterLabel]";
				text2 = "[betrayalOfferFailedBecauseRecruitedLetterText]";
			}
			else
			{
				text = "[betrayalOfferFailedLetterLabel]";
				text2 = "[betrayalOfferFailedLetterText]";
			}
			quest.DestroyThingsOrPassToWorld(betrayalRewardThings, null, questLookTargets: true, QuestPart.SignalListenMode.Always);
			Quest quest2 = quest;
			LetterDef negativeEvent = LetterDefOf.NegativeEvent;
			string label = text;
			quest2.Letter(negativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.Always, null, filterDeadPawnsFromLookTargets: false, text2, null, label);
			quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.Always);
		}, null, new List<string> { item, item3, item4, item5, item6 }, null, QuestPart.SignalListenMode.Always);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (slate.Get<Pawn>("rewardGiver") != null && slate.TryGet<FloatRange>("marketValueRange", out var _) && slate.Get<Faction>("faction") != null)
		{
			return QuestGen_Get.GetMap() != null;
		}
		return false;
	}
}
