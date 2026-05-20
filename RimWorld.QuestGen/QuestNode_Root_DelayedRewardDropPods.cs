using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_DelayedRewardDropPods : QuestNode
{
	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		List<Thing> rewards = slate.Get<List<Thing>>("rewards");
		Faction faction = slate.Get<Faction>("faction");
		Pawn pawn = slate.Get<Pawn>("giver");
		int delayTicks = slate.Get("delayTicks", 0);
		string customerLetterLabel = slate.Get<string>("customLetterLabel");
		string customerLetterText = slate.Get<string>("customLetterText");
		if (pawn != null)
		{
			quest.ReservePawns(Gen.YieldSingle(pawn));
		}
		if (faction != null)
		{
			quest.ReserveFaction(faction);
		}
		quest.Delay(delayTicks, delegate
		{
			quest.DropPods(map.Parent, rewards, customerLetterLabel, null, customerLetterText, null, true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.OngoingOnly, null, destroyItemsOnCleanup: true, dropAllInSamePod: false, allowFogged: false, canRetargetAnyMap: true);
			QuestGen_End.End(quest, QuestEndOutcome.Unknown);
		}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, "RewardDelay");
	}

	protected override bool TestRunInt(Slate slate)
	{
		return slate.Get<List<Thing>>("rewards") != null;
	}
}
