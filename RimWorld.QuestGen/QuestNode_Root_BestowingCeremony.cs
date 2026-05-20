using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_Root_BestowingCeremony : QuestNode
{
	public const string QuestTag = "Bestowing";

	private bool TryGetCeremonyTarget(Slate slate, out Pawn pawn, out Faction bestowingFaction)
	{
		slate.TryGet<Faction>("bestowingFaction", out bestowingFaction);
		if (slate.TryGet<Pawn>("titleHolder", out pawn) && pawn.Faction != null && pawn.Faction.IsPlayer)
		{
			if (bestowingFaction != null)
			{
				return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, bestowingFaction);
			}
			return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, out bestowingFaction);
		}
		pawn = null;
		foreach (Map map in Find.Maps)
		{
			if (!map.IsPlayerHome)
			{
				continue;
			}
			foreach (Pawn allPawn in map.mapPawns.AllPawns)
			{
				if (allPawn.Faction != null && allPawn.Faction.IsPlayer)
				{
					if (bestowingFaction != null)
					{
						return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(allPawn, bestowingFaction);
					}
					return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(allPawn, out bestowingFaction);
				}
			}
		}
		bestowingFaction = null;
		return false;
	}

	protected override void RunInt()
	{
		if (!ModLister.CheckRoyalty("Bestowing ceremony"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		if (!TryGetCeremonyTarget(QuestGen.slate, out var pawn, out var bestowingFaction))
		{
			return;
		}
		RoyalTitleDef titleAwardedWhenUpdating = pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction));
		string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("Bestowing");
		string text2 = QuestGenUtility.QuestTagSignal(text, "CeremonyExpired");
		string inSignal = QuestGenUtility.QuestTagSignal(text, "CeremonyFailed");
		string inSignal2 = QuestGenUtility.QuestTagSignal(text, "CeremonyDone");
		string inSignal3 = QuestGenUtility.QuestTagSignal(text, "BeingAttacked");
		string inSignal4 = QuestGenUtility.QuestTagSignal(text, "Fleeing");
		string inSignal5 = QuestGenUtility.QuestTagSignal(text, "TitleAwardedWhenUpdatingChanged");
		Thing thing = QuestGen_Shuttle.GenerateShuttle(bestowingFaction);
		Pawn pawn2 = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
		{
			mustBeOfKind = PawnKindDefOf.Empire_Royal_Bestower,
			canGeneratePawn = true,
			mustBeOfFaction = bestowingFaction,
			mustBeWorldPawn = true,
			ifWorldPawnThenMustBeFree = true,
			redressPawn = true
		});
		QuestUtility.AddQuestTag(ref thing.questTags, text);
		QuestUtility.AddQuestTag(ref pawn.questTags, text);
		ThingOwner<Thing> innerContainer = pawn2.inventory.innerContainer;
		for (int num = innerContainer.Count - 1; num >= 0; num--)
		{
			if (innerContainer[num].def == ThingDefOf.PsychicAmplifier)
			{
				Thing thing2 = innerContainer[num];
				innerContainer.RemoveAt(num);
				thing2.Destroy();
			}
		}
		for (int i = 0; i < 2; i++)
		{
			innerContainer.TryAdd(ThingMaker.MakeThing(ThingDefOf.PsychicAmplifier), 1);
		}
		List<Pawn> list = new List<Pawn>();
		list.Add(pawn2);
		slate.Set("shuttleContents", list);
		slate.Set("shuttle", thing);
		slate.Set("target", pawn);
		slate.Set("bestower", pawn2);
		slate.Set("bestowingFaction", bestowingFaction);
		List<Pawn> list2 = new List<Pawn>();
		for (int j = 0; j < 6; j++)
		{
			Pawn item = quest.GeneratePawn(PawnKindDefOf.Empire_Fighter_Janissary, bestowingFaction);
			list.Add(item);
			list2.Add(item);
		}
		quest.EnsureNotDowned(list);
		slate.Set("defenders", list2);
		thing.TryGetComp<CompShuttle>().requiredPawns = list;
		TransportShip transportShip = quest.GenerateTransportShip(TransportShipDefOf.Ship_Shuttle, list, thing).transportShip;
		Quest quest2 = quest;
		Pawn mapOfPawn = pawn;
		Faction ofEmpire = Faction.OfEmpire;
		quest2.AddShipJob_Arrive(transportShip, null, mapOfPawn, null, ShipJobStartMode.Instant, ofEmpire);
		quest.AddShipJob(transportShip, ShipJobDefOf.Unload);
		quest.AddShipJob_WaitForever(transportShip, leaveImmediatelyWhenSatisfied: true, showGizmos: false, list.Cast<Thing>().ToList()).sendAwayIfAnyDespawnedDownedOrDead = new List<Thing> { pawn2 };
		QuestUtility.AddQuestTag(ref transportShip.questTags, text);
		quest.FactionGoodwillChange(bestowingFaction, -5, QuestGenUtility.HardcodedSignalWithQuestID("defenders.Killed"), canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.QuestPawnLost);
		QuestPart_BestowingCeremony questPart_BestowingCeremony = new QuestPart_BestowingCeremony();
		questPart_BestowingCeremony.inSignal = QuestGen.slate.Get<string>("inSignal");
		questPart_BestowingCeremony.pawns.Add(pawn2);
		questPart_BestowingCeremony.mapOfPawn = pawn;
		questPart_BestowingCeremony.faction = pawn2.Faction;
		questPart_BestowingCeremony.bestower = pawn2;
		questPart_BestowingCeremony.target = pawn;
		questPart_BestowingCeremony.shuttle = thing;
		questPart_BestowingCeremony.questTag = text;
		quest.AddPart(questPart_BestowingCeremony);
		QuestPart_EscortPawn questPart_EscortPawn = new QuestPart_EscortPawn();
		questPart_EscortPawn.inSignal = QuestGen.slate.Get<string>("inSignal");
		questPart_EscortPawn.escortee = pawn2;
		questPart_EscortPawn.pawns.AddRange(list2);
		questPart_EscortPawn.mapOfPawn = pawn;
		questPart_EscortPawn.faction = pawn2.Faction;
		questPart_EscortPawn.shuttle = thing;
		questPart_EscortPawn.questTag = text;
		questPart_EscortPawn.leavingDangerMessage = "MessageBestowingDanger".Translate();
		quest.AddPart(questPart_EscortPawn);
		string inSignal6 = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Killed");
		quest.FactionGoodwillChange(bestowingFaction, 0, inSignal6, canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.ShuttleDestroyed, QuestPart.SignalListenMode.OngoingOnly, ensureMakesHostile: true);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal6, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		QuestPart_RequirementsToAcceptThroneRoom questPart_RequirementsToAcceptThroneRoom = new QuestPart_RequirementsToAcceptThroneRoom();
		questPart_RequirementsToAcceptThroneRoom.faction = bestowingFaction;
		questPart_RequirementsToAcceptThroneRoom.forPawn = pawn;
		questPart_RequirementsToAcceptThroneRoom.forTitle = titleAwardedWhenUpdating;
		quest.AddPart(questPart_RequirementsToAcceptThroneRoom);
		QuestPart_RequirementsToAcceptPawnOnColonyMap questPart_RequirementsToAcceptPawnOnColonyMap = new QuestPart_RequirementsToAcceptPawnOnColonyMap();
		questPart_RequirementsToAcceptPawnOnColonyMap.pawn = pawn;
		quest.AddPart(questPart_RequirementsToAcceptPawnOnColonyMap);
		QuestPart_RequirementsToAcceptNoDanger questPart_RequirementsToAcceptNoDanger = new QuestPart_RequirementsToAcceptNoDanger();
		questPart_RequirementsToAcceptNoDanger.mapPawn = pawn;
		questPart_RequirementsToAcceptNoDanger.dangerTo = bestowingFaction;
		quest.AddPart(questPart_RequirementsToAcceptNoDanger);
		quest.AddPart(new QuestPart_RequirementsToAcceptNoOngoingBestowingCeremony());
		string inSignal7 = QuestGenUtility.HardcodedSignalWithQuestID("shuttleContents.Recruited");
		string inSignal8 = QuestGenUtility.HardcodedSignalWithQuestID("bestowingFaction.BecameHostileToPlayer");
		quest.Signal(inSignal7, delegate
		{
			quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		});
		quest.Bestowing_TargetChangedTitle(pawn, pawn2, titleAwardedWhenUpdating, inSignal5);
		quest.Letter(LetterDefOf.NegativeEvent, text2, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelBestowingCeremonyExpired".Translate(), text: "LetterTextBestowingCeremonyExpired".Translate(pawn.Named("TARGET")));
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("target.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("bestower.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("bestower.LeftBehind"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("shuttle.LeftBehind"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, text2);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal8, QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal3, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal4, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Success, 0, null, inSignal2);
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item2 = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)new Reward_BestowingCeremony
			{
				targetPawnName = pawn.NameShortColored.Resolve(),
				titleName = titleAwardedWhenUpdating.GetLabelCapFor(pawn),
				awardingFaction = bestowingFaction,
				givePsylink = (titleAwardedWhenUpdating.maxPsylinkLevel > pawn.GetPsylinkLevel()),
				royalTitle = titleAwardedWhenUpdating
			} }
		};
		questPart_Choice.choices.Add(item2);
		List<Rule> list3 = new List<Rule>();
		list3.AddRange(GrammarUtility.RulesForPawn("pawn", pawn));
		list3.Add(new Rule_String("newTitle", titleAwardedWhenUpdating.GetLabelCapFor(pawn)));
		QuestGen.AddQuestNameRules(list3);
		List<Rule> list4 = new List<Rule>();
		list4.AddRange(GrammarUtility.RulesForFaction("faction", bestowingFaction));
		list4.AddRange(GrammarUtility.RulesForPawn("pawn", pawn));
		list4.Add(new Rule_String("newTitle", pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction)).GetLabelFor(pawn)));
		list4.Add(new Rule_String("psylinkLevel", titleAwardedWhenUpdating.maxPsylinkLevel.ToString()));
		QuestGen.AddQuestDescriptionRules(list4);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!TryGetCeremonyTarget(slate, out var pawn, out var bestowingFaction) || bestowingFaction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (!QuestGen_Pawns.GetPawnTest(new QuestGen_Pawns.GetPawnParms
		{
			mustBeOfKind = PawnKindDefOf.Empire_Royal_Bestower,
			canGeneratePawn = true,
			mustBeOfFaction = bestowingFaction
		}, out pawn))
		{
			return false;
		}
		return true;
	}
}
