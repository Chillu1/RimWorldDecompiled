using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_RefugeeDelayedReward : QuestNode
	{
		protected override void RunInt()
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = QuestGen_Get.GetMap();
			Faction faction = slate.Get<Faction>("faction");
			FloatRange marketValueRange = slate.Get<FloatRange>("marketValueRange");
			Pawn val = slate.Get<Pawn>("rewardGiver");
			quest.ReservePawns(Gen.YieldSingle(val));
			quest.ReserveFaction(faction);
			int num = Rand.Range(5, 20) * 60000;
			slate.Set("rewardDelayTicks", num);
			quest.Delay(num, delegate
			{
				ThingSetMakerParams parms = default(ThingSetMakerParams);
				parms.totalMarketValueRange = marketValueRange;
				parms.qualityGenerator = QualityGenerator.Reward;
				parms.makingFaction = faction;
				List<Thing> list = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
				slate.Set("listOfRewards", GenLabel.ThingsLabel(list));
				quest.DropPods(map.Parent, list, null, null, "[rewardLetterText]", null, true);
				QuestGen_End.End(quest, QuestEndOutcome.Unknown);
			}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, "RewardDelay");
		}

		protected override bool TestRunInt(Slate slate)
		{
			if (slate.Get<Pawn>("rewardGiver") != null && slate.TryGet<FloatRange>("marketValueRange", out var _))
			{
				return slate.Get<Faction>("faction") != null;
			}
			return false;
		}
	}
}
