using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Hack_AncientComplex : QuestNode_Root_AncientComplex
{
	private const int MinDistanceFromColony = 2;

	private const int MaxDistanceFromColony = 10;

	private static IntRange HackDefenceRange = new IntRange(300, 1800);

	private const float MinMaxHackDefenceChance = 0.5f;

	private static readonly FloatRange RandomRaidPointsFactorRange = new FloatRange(0.25f, 0.35f);

	private const float ChanceToSpawnAllTerminalsHackedRaid = 0.5f;

	protected override void RunInt()
	{
		if (ModLister.CheckIdeology("Ancient complex rescue"))
		{
			Slate slate = QuestGen.slate;
			Quest quest = QuestGen.quest;
			Map map = QuestGen_Get.GetMap();
			float num = slate.Get("points", 0f);
			Precept_Relic precept_Relic = slate.Get<Precept_Relic>("relic");
			TryFindSiteTile(out var tile);
			TryFindEnemyFaction(out var enemyFaction);
			if (precept_Relic == null)
			{
				precept_Relic = Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>().RandomElement();
				Log.Warning("Ancient Complex quest requires relic from parent quest. None found so picking random player relic");
			}
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("terminals.Destroyed");
			string inSignal2 = QuestGen.GenerateNewSignal("TerminalHacked");
			string inSignal3 = QuestGen.GenerateNewSignal("AllTerminalsHacked");
			QuestGen.GenerateNewSignal("RaidArrives");
			string inSignal4 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
			AddQuestRelicParts(precept_Relic, quest);
			LayoutStructureSketch layoutStructureSketch = QuestSetupComplex(quest, num);
			float num2 = (Find.Storyteller.difficulty.allowViolentQuests ? QuestNode_Root_AncientComplex.ThreatPointsOverPointsCurve.Evaluate(num) : 0f);
			Site site = QuestGen_Sites.GenerateSite(Gen.YieldSingle(new SitePartDefWithParams(parms: new SitePartParams
			{
				ancientLayoutStructureSketch = layoutStructureSketch,
				ancientComplexRewardMaker = ThingSetMakerDefOf.MapGen_AncientComplexRoomLoot_Default,
				threatPoints = num2
			}, def: SitePartDefOf.AncientComplex)), tile, Faction.OfAncients);
			quest.SpawnWorldObject(site);
			TimedDetectionRaids component = site.GetComponent<TimedDetectionRaids>();
			if (component != null)
			{
				component.alertRaidsArrivingIn = true;
			}
			quest.Message("[terminalHackedMessage]", null, getLookTargetsFromSignal: true, null, null, inSignal2);
			quest.Message("[allTerminalsHackedMessage]", MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, null, inSignal3);
			if (Find.Storyteller.difficulty.allowViolentQuests && Rand.Chance(0.5f))
			{
				quest.RandomRaid(site, RandomRaidPointsFactorRange * num2, enemyFaction, inSignal3, PawnsArrivalModeDefOf.EdgeWalkIn, RaidStrategyDefOf.ImmediateAttack);
			}
			QuestPart_Filter_Hacked questPart_Filter_Hacked = new QuestPart_Filter_Hacked
			{
				inSignal = inSignal,
				outSignalElse = QuestGen.GenerateNewSignal("FailQuestTerminalDestroyed")
			};
			quest.AddPart(questPart_Filter_Hacked);
			quest.End(QuestEndOutcome.Success, 0, null, inSignal3, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal4, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, questPart_Filter_Hacked.outSignalElse, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			slate.Set("terminals", layoutStructureSketch.thingsToSpawn);
			slate.Set("terminalCount", layoutStructureSketch.thingsToSpawn.Count);
			slate.Set("map", map);
			slate.Set("relic", precept_Relic);
			slate.Set("site", site);
		}
	}

	public override LayoutStructureSketch QuestSetupComplex(Quest quest, float points)
	{
		Precept_Relic orAddRelicFromQuest = GetOrAddRelicFromQuest(quest);
		LayoutStructureSketch layoutStructureSketch = GenerateStructureSketch(points);
		layoutStructureSketch.thingDiscoveredMessage = "MessageAncientTerminalDiscovered".Translate(orAddRelicFromQuest.Label);
		string item = QuestGen.GenerateNewSignal("AllTerminalsHacked", ensureUnique: false);
		string outSignalAny = QuestGen.GenerateNewSignal("TerminalHacked", ensureUnique: false);
		List<string> list = new List<string>();
		for (int i = 0; i < layoutStructureSketch.thingsToSpawn.Count; i++)
		{
			Thing thing = layoutStructureSketch.thingsToSpawn[i];
			string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("terminal" + i);
			QuestUtility.AddQuestTag(thing, text);
			string item2 = QuestGenUtility.HardcodedSignalWithQuestID(text + ".Hacked");
			list.Add(item2);
			thing.TryGetComp<CompHackable>().defence = (Rand.Chance(0.5f) ? HackDefenceRange.min : HackDefenceRange.max);
		}
		if (!quest.TryGetFirstPartOfType<QuestPart_PassAllActivable>(out var part))
		{
			part = quest.AddPart<QuestPart_PassAllActivable>();
			part.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
			part.expiryInfoPartKey = "TerminalsHacked";
		}
		part.inSignals = list;
		part.outSignalsCompleted.Add(item);
		part.outSignalAny = outSignalAny;
		return layoutStructureSketch;
	}

	private static void AddQuestRelicParts(Precept_Relic relic, Quest quest)
	{
		Reward_RelicInfo item = new Reward_RelicInfo
		{
			relic = relic,
			quest = quest
		};
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item2 = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)item }
		};
		questPart_Choice.choices.Add(item2);
	}

	private static Precept_Relic GetOrAddRelicFromQuest(Quest quest)
	{
		foreach (QuestPart_Choice.Choice choice in quest.GetFirstOrAddPart<QuestPart_Choice>().choices)
		{
			foreach (Reward reward in choice.rewards)
			{
				if (reward is Reward_RelicInfo reward_RelicInfo)
				{
					return reward_RelicInfo.relic;
				}
			}
		}
		Log.Error("Failed to get relic from quest part, quest = " + quest.ToStringSafe());
		return null;
	}

	private bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
	{
		return TileFinder.TryFindNewSiteTile(out tile, 2, 10, allowCaravans: false, QuestNode_Root_AncientComplex.AllowedLandmarks, 0.5f, canSelectComboLandmarks: true, TileFinderMode.Near, exitOnFirstTileFound);
	}

	private bool TryFindEnemyFaction(out Faction enemyFaction)
	{
		enemyFaction = Find.FactionManager.RandomRaidableEnemyFaction();
		return enemyFaction != null;
	}

	protected override bool TestRunInt(Slate slate)
	{
		Faction enemyFaction;
		if (TryFindSiteTile(out var _, exitOnFirstTileFound: true))
		{
			return TryFindEnemyFaction(out enemyFaction);
		}
		return false;
	}
}
