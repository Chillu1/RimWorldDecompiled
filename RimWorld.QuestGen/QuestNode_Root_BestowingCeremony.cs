using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
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
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Bestowing ceremony is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 3454535);
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
			string inSignal3 = QuestGenUtility.QuestTagSignal(text, "TitleAwardedWhenUpdatingChanged");
			Thing thing = QuestGen_Shuttle.GenerateShuttle(bestowingFaction, null, null, acceptColonists: false, onlyAcceptColonists: false, onlyAcceptHealthy: false, 0, dropEverythingIfUnsatisfied: false, leaveImmediatelyWhenSatisfied: true, dropEverythingOnArrival: true, stayAfterDroppedEverythingOnArrival: true);
			Pawn pawn2 = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
			{
				mustBeOfKind = PawnKindDefOf.Empire_Royal_Bestower,
				canGeneratePawn = true,
				mustBeOfFaction = bestowingFaction,
				mustBeWorldPawn = true
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
			int num2 = titleAwardedWhenUpdating.maxPsylinkLevel - pawn.GetPsylinkLevel();
			for (int i = 0; i < num2 + 1; i++)
			{
				innerContainer.TryAdd(ThingMaker.MakeThing(ThingDefOf.PsychicAmplifier), 1);
			}
			List<Pawn> list = new List<Pawn>();
			list.Add(pawn2);
			slate.Set("shuttleContents", list);
			slate.Set("shuttle", thing);
			slate.Set("target", pawn);
			slate.Set("bestowingFaction", bestowingFaction);
			List<Pawn> list2 = new List<Pawn>();
			for (int j = 0; j < 6; j++)
			{
				Pawn item = quest.GeneratePawn(PawnKindDefOf.Empire_Fighter_Janissary, bestowingFaction);
				list.Add(item);
				list2.Add(item);
			}
			slate.Set("defenders", list2);
			CompShuttle compShuttle = thing.TryGetComp<CompShuttle>();
			compShuttle.requiredPawns = list;
			compShuttle.sendAwayIfAllDespawned = list.Cast<Thing>().ToList();
			compShuttle.sendAwayIfAllPawnsLeftToLoadAreNotOfFaction = bestowingFaction;
			quest.AddContentsToShuttle(thing, list);
			quest.SpawnSkyfaller(null, ThingDefOf.ShuttleIncoming, Gen.YieldSingle(thing), Faction.OfPlayer, null, null, lookForSafeSpot: true, tryLandInShipLandingZone: true, null, pawn);
			quest.FactionGoodwillChange(bestowingFaction, -5, QuestGenUtility.HardcodedSignalWithQuestID("defenders.Killed"), canSendMessage: true, canSendHostilityLetter: true, "GoodwillChangeReason_AttackedFaction".Translate(bestowingFaction));
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
			quest.AddPart(questPart_EscortPawn);
			string inSignal4 = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Killed");
			quest.SetFactionRelations(bestowingFaction, FactionRelationKind.Hostile, inSignal4);
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal4, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			QuestPart_RequirementsToAcceptThroneRoom questPart_RequirementsToAcceptThroneRoom = new QuestPart_RequirementsToAcceptThroneRoom();
			questPart_RequirementsToAcceptThroneRoom.faction = bestowingFaction;
			questPart_RequirementsToAcceptThroneRoom.forPawn = pawn;
			questPart_RequirementsToAcceptThroneRoom.forTitle = titleAwardedWhenUpdating;
			quest.AddPart(questPart_RequirementsToAcceptThroneRoom);
			QuestPart_RequirementsToAcceptPawnOnColonyMap questPart_RequirementsToAcceptPawnOnColonyMap = new QuestPart_RequirementsToAcceptPawnOnColonyMap();
			questPart_RequirementsToAcceptPawnOnColonyMap.pawn = pawn;
			quest.AddPart(questPart_RequirementsToAcceptPawnOnColonyMap);
			QuestPart_RequirementsToAcceptNoDanger questPart_RequirementsToAcceptNoDanger = new QuestPart_RequirementsToAcceptNoDanger();
			questPart_RequirementsToAcceptNoDanger.map = pawn.Map;
			questPart_RequirementsToAcceptNoDanger.dangerTo = bestowingFaction;
			quest.AddPart(questPart_RequirementsToAcceptNoDanger);
			string inSignal5 = QuestGenUtility.HardcodedSignalWithQuestID("shuttleContents.Recruited");
			string inSignal6 = QuestGenUtility.HardcodedSignalWithQuestID("bestowingFaction.BecameHostileToPlayer");
			quest.Signal(inSignal5, delegate
			{
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			});
			quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("target.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
			quest.Letter(LetterDefOf.NegativeEvent, text2, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelBestowingCeremonyExpired".Translate(), text: "LetterTextBestowingCeremonyExpired".Translate(pawn.Named("TARGET")));
			quest.End(QuestEndOutcome.Fail, 0, null, text2);
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal6, QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal3, QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
			quest.End(QuestEndOutcome.Success, 0, null, inSignal2);
			QuestPart_Choice questPart_Choice = quest.RewardChoice();
			QuestPart_Choice.Choice item2 = new QuestPart_Choice.Choice
			{
				rewards = 
				{
					(Reward)new Reward_BestowingCeremony
					{
						targetPawnName = pawn.NameShortColored.Resolve(),
						titleName = titleAwardedWhenUpdating.GetLabelCapFor(pawn),
						awardingFaction = bestowingFaction,
						givePsylink = (titleAwardedWhenUpdating.maxPsylinkLevel > pawn.GetPsylinkLevel()),
						royalTitle = titleAwardedWhenUpdating
					}
				}
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
			if (!TryGetCeremonyTarget(slate, out var _, out var bestowingFaction) || bestowingFaction.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			QuestGen_Pawns.GetPawnParms parms = default(QuestGen_Pawns.GetPawnParms);
			parms.mustBeOfKind = PawnKindDefOf.Empire_Royal_Bestower;
			parms.canGeneratePawn = true;
			parms.mustBeOfFaction = bestowingFaction;
			if (!QuestGen_Pawns.GetPawnTest(parms, out var _))
			{
				return false;
			}
			return true;
		}
	}
}
