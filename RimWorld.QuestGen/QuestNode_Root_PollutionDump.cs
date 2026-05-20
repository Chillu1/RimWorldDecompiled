using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_PollutionDump : QuestNode
{
	private const int DropPodsDelayTicks = 2500;

	private static readonly SimpleCurve WastepackCountOverPointsCurve = new SimpleCurve
	{
		new CurvePoint(200f, 90f),
		new CurvePoint(400f, 150f),
		new CurvePoint(800f, 225f),
		new CurvePoint(1600f, 325f),
		new CurvePoint(3200f, 400f),
		new CurvePoint(20000f, 1000f)
	};

	protected override bool TestRunInt(Slate slate)
	{
		if (QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true) != null)
		{
			return FindAsker() != null;
		}
		return false;
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
		slate.Set("map", map);
		float x = slate.Get("points", 0f);
		Pawn asker = FindAsker();
		int wastepackCount = Mathf.RoundToInt(WastepackCountOverPointsCurve.Evaluate(x));
		quest.Delay(2500, delegate
		{
			List<Thing> list = new List<Thing>();
			int num = wastepackCount;
			while (num > 0)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Wastepack);
				thing.stackCount = Mathf.Min(num, thing.def.stackLimit);
				list.Add(thing);
				num -= thing.stackCount;
			}
			quest.DropPods(map.Parent, list, "[wastepacksLetterLabel]", null, "[wastepacksLetterText]", null, true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.OngoingOnly, null, destroyItemsOnCleanup: true, dropAllInSamePod: true, allowFogged: false, canRetargetAnyMap: true);
			QuestScriptDefOf.Util_GetDefaultRewardValueFromPoints.Run();
			float rewardValue = slate.Get("rewardValue", 0f);
			Quest quest2 = quest;
			RewardsGeneratorParams parms = new RewardsGeneratorParams
			{
				rewardValue = rewardValue,
				thingRewardItemsOnly = true
			};
			Pawn asker2 = asker;
			quest2.GiveRewards(parms, null, null, null, null, null, null, null, null, addCampLootReward: false, asker2);
			quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, null, tickHistorically: false, QuestPart.SignalListenMode.OngoingOnly, waitUntilPlayerHasHomeMap: true);
		slate.Set("asker", asker);
		slate.Set("askerIsNull", asker == null);
		slate.Set("wastepackCount", wastepackCount);
	}

	private Pawn FindAsker()
	{
		if (Find.FactionManager.AllFactionsVisible.Where((Faction f) => f.def.humanlikeFaction && !f.IsPlayer && !f.HostileTo(Faction.OfPlayer) && (int)f.def.techLevel > 2 && f.leader != null && !f.temporary && !f.Hidden && f.leader.Faction == f).TryRandomElement(out var result))
		{
			return result.leader;
		}
		return null;
	}
}
