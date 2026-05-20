using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Hack_Spacedrone : QuestNode
{
	private const int QuestStartDelay = 120;

	private static IntRange RaidDelayTicksRange = new IntRange(18000, 30000);

	private static IntRange RaidIntervalTicksRange = new IntRange(17500, 22500);

	private static float MinRaidThreatPointsFactor = 0.3f;

	private static float MaxRaidThreatPointsFactor = 0.55f;

	private static int SpacedroneDestroyDelayTicks = 1800000;

	protected override void RunInt()
	{
		if (!ModLister.CheckIdeology("Spacedrone hack"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		float num = slate.Get("points", 0f);
		Precept_Relic precept_Relic = slate.Get<Precept_Relic>("relic");
		bool allowViolentQuests = Find.Storyteller.difficulty.allowViolentQuests;
		QuestPart_SpawnSpaceDrone.TryFindSpacedronePosition(map, out var landingSpot);
		Thing spacedrone = ThingMaker.MakeThing(ThingDefOf.Spacedrone);
		string text = QuestGen.GenerateNewSignal("RaidsDelay");
		string spacedroneDestroyedSignal = QuestGenUtility.HardcodedSignalWithQuestID("spacedrone.Destroyed");
		string spacedroneHackedSignal = QuestGenUtility.HardcodedSignalWithQuestID("spacedrone.Hacked");
		string triggerRaidSignal = QuestGen.GenerateNewSignal("TriggerRaid");
		string text2 = QuestGen.GenerateNewSignal("QuestEndSuccess");
		string text3 = QuestGen.GenerateNewSignal("QuestEndFailure");
		string text4 = QuestGen.GenerateNewSignal("SpacedroneDelayDestroy");
		if (precept_Relic == null)
		{
			precept_Relic = Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>().RandomElement();
			Log.Warning("Spacedrone Hack quest requires relic from parent quest. None found so picking random player relic");
		}
		quest.Delay(120, delegate
		{
			quest.Letter(LetterDefOf.NeutralEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "LetterLabelSpacedroneIncoming".Translate(), text: "LetterTextSpacedroneIncoming".Translate(), lookTargets: Gen.YieldSingle(spacedrone));
			quest.SpawnSpaceDrone(map, ThingDefOf.SpacedroneIncoming, Gen.YieldSingle(spacedrone), null, landingSpot);
		}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, null, tickHistorically: false, QuestPart.SignalListenMode.OngoingOnly, waitUntilPlayerHasHomeMap: true);
		if (allowViolentQuests)
		{
			TryFindEnemyFaction(out var enemyFaction);
			QuestPart_FactionGoodwillLocked questPart_FactionGoodwillLocked = new QuestPart_FactionGoodwillLocked();
			questPart_FactionGoodwillLocked.faction1 = Faction.OfPlayer;
			questPart_FactionGoodwillLocked.faction2 = enemyFaction;
			questPart_FactionGoodwillLocked.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
			quest.AddPart(questPart_FactionGoodwillLocked);
			QuestPart_Delay questPart_Delay = new QuestPart_Delay();
			questPart_Delay.delayTicks = RaidDelayTicksRange.RandomInRange;
			questPart_Delay.alertLabel = "QuestPartRaidsDelay".Translate();
			questPart_Delay.alertExplanation = "QuestPartRaidsDelayDesc".Translate();
			questPart_Delay.ticksLeftAlertCritical = 60000;
			questPart_Delay.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
			questPart_Delay.alertCulprits.Add(spacedrone);
			questPart_Delay.isBad = true;
			questPart_Delay.outSignalsCompleted.Add(text);
			questPart_Delay.waitUntilPlayerHasHomeMap = true;
			quest.AddPart(questPart_Delay);
			quest.Signal(text, delegate
			{
				QuestPart_PassOutInterval part = new QuestPart_PassOutInterval
				{
					inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
					outSignals = { triggerRaidSignal },
					inSignalsDisable = { spacedroneHackedSignal, spacedroneDestroyedSignal },
					ticksInterval = RaidIntervalTicksRange
				};
				quest.AddPart(part);
			});
			QuestPart_RandomRaid questPart_RandomRaid = new QuestPart_RandomRaid();
			questPart_RandomRaid.inSignal = triggerRaidSignal;
			questPart_RandomRaid.mapParent = map.Parent;
			questPart_RandomRaid.pointsRange = new FloatRange(num * MinRaidThreatPointsFactor, num * MaxRaidThreatPointsFactor);
			questPart_RandomRaid.faction = enemyFaction;
			questPart_RandomRaid.arrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
			questPart_RandomRaid.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			questPart_RandomRaid.attackTargets = new List<Thing> { spacedrone };
			questPart_RandomRaid.generateFightersOnly = true;
			questPart_RandomRaid.customLetterLabel = "Raid".Translate() + ": " + enemyFaction.Name;
			questPart_RandomRaid.customLetterText = "LetterTextRaidSpacedrone".Translate(enemyFaction.NameColored, spacedrone.LabelCap).Resolve();
			questPart_RandomRaid.fallbackToPlayerHomeMap = true;
			quest.AddPart(questPart_RandomRaid);
			slate.Set("enemyFaction", enemyFaction);
			slate.Set("enemyFactionNeolithic", enemyFaction.def.techLevel == TechLevel.Neolithic);
		}
		Quest quest2 = quest;
		int spacedroneDestroyDelayTicks = SpacedroneDestroyDelayTicks;
		Action inner = delegate
		{
		};
		string expiryInfoPart = "SpacedroneSelfDestructsIn".Translate();
		quest2.Delay(spacedroneDestroyDelayTicks, inner, null, null, text4, reactivatable: false, null, null, isQuestTimeout: false, expiryInfoPart);
		quest.Letter(LetterDefOf.PositiveEvent, spacedroneHackedSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: string.Format("{0}: {1}", "Hacked".Translate(), spacedrone.LabelCap), text: "LetterTextSpacedroneHacked".Translate(spacedrone.LabelCap, precept_Relic.ThingDef.LabelCap), lookTargets: Gen.YieldSingle(spacedrone));
		quest.AnySignal(new string[2] { text4, spacedroneHackedSignal }, delegate
		{
			quest.StartWick(spacedrone);
		});
		QuestPart_Filter_AllThingsHacked questPart_Filter_AllThingsHacked = new QuestPart_Filter_AllThingsHacked();
		questPart_Filter_AllThingsHacked.things.Add(spacedrone);
		questPart_Filter_AllThingsHacked.inSignal = spacedroneDestroyedSignal;
		questPart_Filter_AllThingsHacked.outSignal = text2;
		questPart_Filter_AllThingsHacked.outSignalElse = text3;
		quest.AddPart(questPart_Filter_AllThingsHacked);
		Reward_RelicInfo reward_RelicInfo = new Reward_RelicInfo();
		reward_RelicInfo.relic = precept_Relic;
		reward_RelicInfo.quest = quest;
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)reward_RelicInfo }
		};
		questPart_Choice.choices.Add(item);
		quest.End(QuestEndOutcome.Fail, 0, null, text3, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Success, 0, null, text2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved"));
		slate.Set("map", map);
		slate.Set("relic", precept_Relic);
		slate.Set("spacedrone", spacedrone);
		slate.Set("raidIntervalAvg", RaidIntervalTicksRange.Average);
		slate.Set("allowViolence", allowViolentQuests);
		slate.Set("destroyDelayTicks", SpacedroneDestroyDelayTicks);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (Find.Storyteller.difficulty.allowViolentQuests && !TryFindEnemyFaction(out var _))
		{
			return false;
		}
		Map map = QuestGen_Get.GetMap();
		IntVec3 spot;
		if (map != null)
		{
			return QuestPart_SpawnSpaceDrone.TryFindSpacedronePosition(map, out spot);
		}
		return false;
	}

	private bool CanUseFaction(Faction f)
	{
		if (!f.temporary && !f.defeated && !f.IsPlayer && (f.def.humanlikeFaction || f == Faction.OfMechanoids) && (!f.Hidden || f == Faction.OfMechanoids))
		{
			return f.HostileTo(Faction.OfPlayer);
		}
		return false;
	}

	private bool TryFindEnemyFaction(out Faction enemyFaction)
	{
		return Find.FactionManager.AllFactions.Where(CanUseFaction).TryRandomElement(out enemyFaction);
	}
}
