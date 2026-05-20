using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_RelicHunt : QuestNode
{
	private const int RelicsInfoRequiredCount = 5;

	private const int MinIntervalTicks = 300000;

	private const int MaxIntervalTicks = 600000;

	private const int MinDistanceFromColony = 2;

	private const int MaxDistanceFromColony = 10;

	private const int SecurityWakeupDelayTicks = 180000;

	private const int SecurityWakupDelayCriticalTicks = 2500;

	private static readonly SimpleCurve ExteriorThreatPointsOverPoints = new SimpleCurve
	{
		new CurvePoint(0f, 500f),
		new CurvePoint(500f, 500f),
		new CurvePoint(10000f, 10000f)
	};

	private static readonly SimpleCurve InteriorThreatPointsOverPoints = new SimpleCurve
	{
		new CurvePoint(0f, 300f),
		new CurvePoint(300f, 300f),
		new CurvePoint(10000f, 5000f)
	};

	protected override void RunInt()
	{
		if (!ModLister.CheckIdeology("Relic hunt rescue"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
		float num = slate.Get("points", 0f);
		Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
		TryGetRandomPlayerRelic(out var relic);
		TryFindSiteTile(out var tile);
		string text = QuestGen.GenerateNewSignal("SubquestsCompleted");
		string awakenSecurityThreatsSignal = QuestGen.GenerateNewSignal("AwakenSecurityThreats");
		QuestGen.GenerateNewSignal("PostMapAdded");
		string text2 = QuestGen.GenerateNewSignal("RelicLostFromMap");
		bool allowViolentQuests = Find.Storyteller.difficulty.allowViolentQuests;
		slate.Set("classicMode", Find.IdeoManager.classicMode);
		slate.Set("relicGlobalCount", primaryIdeo.PreceptsListForReading.Count((Precept x) => x is Precept_Relic));
		QuestPart_SubquestGenerator_RelicHunt questPart_SubquestGenerator_RelicHunt = new QuestPart_SubquestGenerator_RelicHunt();
		questPart_SubquestGenerator_RelicHunt.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
		questPart_SubquestGenerator_RelicHunt.interval = new IntRange(300000, 600000);
		questPart_SubquestGenerator_RelicHunt.relic = relic;
		questPart_SubquestGenerator_RelicHunt.relicSlateName = "relic";
		questPart_SubquestGenerator_RelicHunt.useMapParentThreatPoints = map?.Parent;
		questPart_SubquestGenerator_RelicHunt.expiryInfoPartKey = "RelicInfoFound";
		questPart_SubquestGenerator_RelicHunt.maxSuccessfulSubquests = 5;
		questPart_SubquestGenerator_RelicHunt.subquestDefs.AddRange(GetAllSubquests(QuestGen.Root));
		questPart_SubquestGenerator_RelicHunt.outSignalsCompleted.Add(text);
		quest.AddPart(questPart_SubquestGenerator_RelicHunt);
		QuestGenUtility.RunAdjustPointsForDistantFight();
		num = slate.Get("points", 0f);
		Thing thing = relic.GenerateRelic();
		QuestGen_Signal.SignalPass(inSignal: QuestGenUtility.HardcodedSignalWithQuestID("relicThing.StartedExtractingFromContainer"), quest: quest, action: null, outSignal: awakenSecurityThreatsSignal);
		Reward_Items item = new Reward_Items
		{
			items = { thing }
		};
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)item }
		};
		questPart_Choice.choices.Add(choice);
		if (Faction.OfPlayer.ideos.FluidIdeo != null)
		{
			choice.rewards.Add(new Reward_DevelopmentPoints(quest));
		}
		float num2 = (allowViolentQuests ? num : 0f);
		SitePartParams sitePartParams = new SitePartParams
		{
			points = num2,
			relicThing = thing,
			triggerSecuritySignal = awakenSecurityThreatsSignal,
			relicLostSignal = text2
		};
		if (num2 > 0f)
		{
			sitePartParams.exteriorThreatPoints = ExteriorThreatPointsOverPoints.Evaluate(num2);
			sitePartParams.interiorThreatPoints = InteriorThreatPointsOverPoints.Evaluate(num2);
		}
		Site site = QuestGen_Sites.GenerateSite(Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.AncientAltar, sitePartParams)), tile, Faction.OfAncientsHostile);
		quest.SpawnWorldObject(site, null, text);
		TaggedString taggedString = "LetterTextRelicFoundLocation".Translate(relic.Label);
		if (allowViolentQuests)
		{
			taggedString += "\n\n" + "LetterTextRelicFoundSecurityThreats".Translate(180000.ToStringTicksToPeriodVague()).Resolve();
		}
		quest.Letter(LetterDefOf.RelicHuntInstallationFound, text, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "LetterLabelRelicFound".Translate() + ": " + relic.Label, text: taggedString.Resolve(), lookTargets: Gen.YieldSingle(site));
		quest.DescriptionPart("[questDescriptionPartBeforeDiscovery]", quest.AddedSignal, text, QuestPart.SignalListenMode.OngoingOrNotYetAccepted);
		if (allowViolentQuests)
		{
			quest.DescriptionPart("RelicHuntFoundRelicSite".Translate(), text);
			QuestPart_Delay part = new QuestPart_Delay
			{
				delayTicks = 180000,
				alertLabel = "AncientAltarThreatsWaking".Translate(),
				alertExplanation = "AncientAltarThreatsWakingDesc".Translate(),
				ticksLeftAlertCritical = 2500,
				inSignalEnable = text,
				alertCulprits = 
				{
					(GlobalTargetInfo)thing,
					(GlobalTargetInfo)site
				},
				isBad = true,
				outSignalsCompleted = { awakenSecurityThreatsSignal }
			};
			quest.AddPart(part);
			string text3 = QuestGen.GenerateNewSignal("ReTriggerSecurityThreats");
			QuestPart_PassWhileActive part2 = new QuestPart_PassWhileActive
			{
				inSignalEnable = awakenSecurityThreatsSignal,
				inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated"),
				outSignal = text3
			};
			quest.AddPart(part2);
			quest.SignalPass(delegate
			{
				quest.SignalPass(null, null, awakenSecurityThreatsSignal);
				quest.Message("MessageAncientAltarThreatsAlerted".Translate(), MessageTypeDefOf.NegativeEvent);
			}, text3);
			quest.AnyHostileThreatToPlayer(site, countDormantPawns: true, delegate
			{
				quest.Message("MessageAncientAltarThreatsWokenUp".Translate(), MessageTypeDefOf.NegativeEvent);
			}, null, awakenSecurityThreatsSignal);
		}
		else
		{
			quest.DescriptionPart("RelicHuntFoundRelicSitePeaceful".Translate(), text);
		}
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("relicThing.Destroyed"), QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Success, 0, null, text2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		slate.Set("ideo", primaryIdeo);
		slate.Set("relic", relic);
		slate.Set("relicThing", thing);
		slate.Set("site", site);
	}

	private bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
	{
		return TileFinder.TryFindNewSiteTile(out tile, 2, 10, allowCaravans: false, null, 0.5f, canSelectComboLandmarks: true, TileFinderMode.Near, exitOnFirstTileFound);
	}

	private bool TryGetRandomPlayerRelic(out Precept_Relic relic)
	{
		return (from p in Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>()
			where p.CanGenerateRelic
			select p).TryRandomElement(out relic);
	}

	private IEnumerable<QuestScriptDef> GetAllSubquests(QuestScriptDef parent)
	{
		return DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef q) => q.epicParent == parent);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (TryGetRandomPlayerRelic(out var _) && TryFindSiteTile(out var _, exitOnFirstTileFound: true))
		{
			return GetAllSubquests(QuestGen.Root).Any();
		}
		return false;
	}
}
