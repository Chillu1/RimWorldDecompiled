using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_PollutionRetaliation : QuestNode
	{
		private static readonly IntRange MaxDelayTicksRange = new IntRange(60000, 180000);

		private static readonly SimpleCurve WastepackCountOverPointsCurve = new SimpleCurve
		{
			new CurvePoint(200f, 40f),
			new CurvePoint(400f, 80f),
			new CurvePoint(800f, 120f),
			new CurvePoint(1600f, 160f),
			new CurvePoint(3200f, 200f),
			new CurvePoint(20000f, 500f)
		};

		protected override bool TestRunInt(Slate slate)
		{
			if (QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true) != null && slate.TryGet<Faction>("enemyFaction", out var var))
			{
				return IsValidFaction(var);
			}
			return false;
		}

		protected override void RunInt()
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = slate.Get<Map>("map");
			float x = slate.Get("points", 0f);
			string completeSignal = QuestGen.GenerateNewSignal("DelayCompleted");
			Pawn sender = GetSender(slate);
			int num = Mathf.RoundToInt(WastepackCountOverPointsCurve.Evaluate(x));
			quest.Delay(MaxDelayTicksRange.RandomInRange, delegate
			{
				quest.SignalPass(null, null, completeSignal);
			}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, null, tickHistorically: false, QuestPart.SignalListenMode.OngoingOnly, waitUntilPlayerHasHomeMap: true).debugLabel = "Arrival delay";
			int num2 = Rand.Range(1, 4);
			int stackCount = num / num2;
			List<Thing> list = new List<Thing>();
			for (int num3 = 0; num3 < num2; num3++)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Wastepack);
				thing.stackCount = stackCount;
				list.Add(thing);
			}
			for (int num4 = 0; num4 < num2; num4++)
			{
				List<Thing> contents = new List<Thing> { list[num4] };
				quest.DropPods(map.Parent, contents, null, null, null, null, false, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, completeSignal, null, QuestPart.SignalListenMode.OngoingOnly, null, destroyItemsOnCleanup: true, dropAllInSamePod: true, allowFogged: false, canRetargetAnyMap: true);
			}
			quest.Letter(LetterDefOf.NegativeEvent, completeSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, list, filterDeadPawnsFromLookTargets: false, "[retaliationLetterText]", null, "[retaliationLetterLabel]");
			quest.End(QuestEndOutcome.Unknown, 0, null, completeSignal);
			slate.Set("sender", sender);
			slate.Set("senderIsNull", sender == null);
			slate.Set("wastepackCount", num);
		}

		private Pawn GetSender(Slate slate)
		{
			if (slate.TryGet<Faction>("enemyFaction", out var var) && IsValidFaction(var))
			{
				return var.leader;
			}
			if (Find.FactionManager.AllFactionsVisible.Where(IsValidFaction).TryRandomElement(out var))
			{
				return var.leader;
			}
			return null;
		}

		private static bool IsValidFaction(Faction faction)
		{
			if (!faction.IsPlayer && faction.HostileTo(Faction.OfPlayer))
			{
				return (int)faction.def.techLevel > 2;
			}
			return false;
		}
	}
}
