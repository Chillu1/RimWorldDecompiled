using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Hack_WorshippedTerminal : QuestNode
{
	private const int MinDistanceFromColony = 2;

	private const int MaxDistanceFromColony = 10;

	private static IntRange HackDefenceRange = new IntRange(10, 100);

	private const int FactionBecomesHostileAfterHours = 10;

	private const float PointsMultiplierRaid = 0.2f;

	private const float MinPointsRaid = 45f;

	protected override void RunInt()
	{
		if (!ModLister.CheckIdeology("Worshipped terminal"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
		QuestGenUtility.RunAdjustPointsForDistantFight();
		float a = slate.Get("points", 0f);
		Precept_Relic precept_Relic = slate.Get<Precept_Relic>("relic");
		slate.Set("playerFaction", Faction.OfPlayer);
		slate.Set("allowViolentQuests", Find.Storyteller.difficulty.allowViolentQuests);
		TryFindSiteTile(out var tile);
		List<FactionRelation> list = new List<FactionRelation>();
		foreach (Faction item2 in Find.FactionManager.AllFactionsListForReading)
		{
			if (!item2.def.PermanentlyHostileTo(FactionDefOf.TribeCivil))
			{
				list.Add(new FactionRelation
				{
					other = item2,
					kind = FactionRelationKind.Neutral
				});
			}
		}
		FactionDef tribeCivil = FactionDefOf.TribeCivil;
		bool? hidden = true;
		FactionGeneratorParms parms = new FactionGeneratorParms(tribeCivil, default(IdeoGenerationParms), hidden);
		parms.ideoGenerationParms = new IdeoGenerationParms(parms.factionDef);
		Faction tribalFaction = FactionGenerator.NewGeneratedFactionWithRelations(parms, list);
		tribalFaction.temporary = true;
		tribalFaction.factionHostileOnHarmByPlayer = Find.Storyteller.difficulty.allowViolentQuests;
		tribalFaction.neverFlee = true;
		Find.FactionManager.Add(tribalFaction);
		quest.ReserveFaction(tribalFaction);
		if (precept_Relic == null)
		{
			precept_Relic = Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>().RandomElement();
			Log.Warning("Worshipped terminal quest requires relic from parent quest. None found so picking random player relic");
		}
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("playerFaction.BuiltBuilding");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("playerFaction.PlacedBlueprint");
		a = Mathf.Max(a, tribalFaction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement_RangedOnly));
		SitePartParams parms2 = new SitePartParams
		{
			points = a,
			threatPoints = a,
			relic = precept_Relic
		};
		Site site = QuestGen_Sites.GenerateSite(Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.WorshippedTerminal, parms2)), tile, tribalFaction);
		site.doorsAlwaysOpenForPlayerPawns = true;
		slate.Set("site", site);
		quest.SpawnWorldObject(site);
		int num = 25000;
		site.GetComponent<TimedMakeFactionHostile>().SetupTimer(num, "WorshippedTerminalFactionBecameHostileTimed".Translate(tribalFaction.Named("FACTION")));
		Thing thing = site.parts[0].things.First((Thing t) => t.def == ThingDefOf.AncientTerminal_Worshipful);
		slate.Set("terminal", thing);
		string terminalDestroyedSignal = QuestGenUtility.HardcodedSignalWithQuestID("terminal.Destroyed");
		string text = QuestGenUtility.HardcodedSignalWithQuestID("terminal.Hacked");
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID("terminal.HackingStarted");
		string text3 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		string inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal3 = QuestGenUtility.HardcodedSignalWithQuestID("tribalFaction.FactionMemberArrested");
		CompHackable compHackable = thing.TryGetComp<CompHackable>();
		compHackable.hackingStartedSignal = text2;
		compHackable.defence = HackDefenceRange.RandomInRange;
		quest.Message("[terminalHackedMessage]", null, getLookTargetsFromSignal: true, null, null, text);
		quest.SetFactionHidden(tribalFaction);
		if (Find.Storyteller.difficulty.allowViolentQuests)
		{
			quest.FactionRelationToPlayerChange(tribalFaction, FactionRelationKind.Hostile, canSendHostilityLetter: false, text2);
			quest.StartRecurringRaids(site, new FloatRange(24f, 24f), 2500, text2);
			quest.BuiltNearSettlement(tribalFaction, site, delegate
			{
				quest.FactionRelationToPlayerChange(tribalFaction, FactionRelationKind.Hostile);
			}, null, inSignal);
			quest.BuiltNearSettlement(tribalFaction, site, delegate
			{
				quest.Message("WarningBuildingCausesHostility".Translate(tribalFaction.Named("FACTION")), MessageTypeDefOf.CautionInput);
			}, null, inSignal2);
			quest.FactionRelationToPlayerChange(tribalFaction, FactionRelationKind.Hostile, canSendHostilityLetter: true, inSignal3);
		}
		Reward_RelicInfo reward_RelicInfo = new Reward_RelicInfo();
		reward_RelicInfo.relic = precept_Relic;
		reward_RelicInfo.quest = quest;
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)reward_RelicInfo }
		};
		questPart_Choice.choices.Add(item);
		quest.SignalPassActivable(delegate
		{
			quest.End(QuestEndOutcome.Fail, 0, null, terminalDestroyedSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		}, null, null, null, null, text);
		quest.SignalPassActivable(delegate
		{
			quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		}, inSignalEnable, text3, null, null, text);
		quest.SignalPassAll(delegate
		{
			quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		}, new List<string> { text, text3 });
		slate.Set("map", map);
		slate.Set("relic", precept_Relic);
		slate.Set("timer", num);
		slate.Set("tribalFaction", tribalFaction);
	}

	private bool TryFindSiteTile(out PlanetTile tile)
	{
		return TileFinder.TryFindNewSiteTile(out tile, 2, 10);
	}

	protected override bool TestRunInt(Slate slate)
	{
		PlanetTile tile;
		return TryFindSiteTile(out tile);
	}
}
