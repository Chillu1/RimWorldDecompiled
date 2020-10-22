using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public abstract class QuestNode_Root_Mission : QuestNode
	{
		public const int MinTilesAwayFromColony = 80;

		public const int MaxTilesAwayFromColony = 85;

		private static readonly SimpleCurve RewardValueCurve = new SimpleCurve
		{
			new CurvePoint(200f, 550f),
			new CurvePoint(400f, 1100f),
			new CurvePoint(800f, 1600f),
			new CurvePoint(1600f, 2600f),
			new CurvePoint(3200f, 3600f),
			new CurvePoint(30000f, 10000f)
		};

		public FloatRange timeLimitDays = new FloatRange(2f, 5f);

		private static List<Map> tmpMaps = new List<Map>();

		protected abstract string QuestTag
		{
			get;
		}

		protected virtual bool AddCampLootReward => false;

		protected virtual bool IsViolent => true;

		protected abstract Pawn GetAsker(Quest quest);

		protected virtual bool CanGetAsker()
		{
			return true;
		}

		protected abstract int GetRequiredPawnCount(int population, float threatPoints);

		protected virtual bool DoesPawnCountAsAvailableForFight(Pawn p)
		{
			return true;
		}

		protected abstract Site GenerateSite(Pawn asker, float threatPoints, int pawnCount, int population, int tile);

		protected virtual bool TryFindSiteTile(out int tile)
		{
			return TileFinder.TryFindNewSiteTile(out tile, 80, 85);
		}

		private void ResolveParameters(Slate slate, out int requiredPawnCount, out int population, out Map colonyMap)
		{
			try
			{
				foreach (Map map in Find.Maps)
				{
					if (map.IsPlayerHome)
					{
						tmpMaps.Add(map);
					}
				}
				colonyMap = tmpMaps.RandomElementWithFallback();
				population = (slate.Exists("population") ? slate.Get("population", 0) : colonyMap.mapPawns.FreeColonists.Where((Pawn c) => DoesPawnCountAsAvailableForFight(c)).Count());
				requiredPawnCount = GetRequiredPawnCount(population, slate.Get("points", 0));
			}
			finally
			{
				tmpMaps.Clear();
			}
		}

		protected override void RunInt()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Missions are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 324345634);
				return;
			}
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(QuestTag);
			int num = slate.Get("points", 0);
			Pawn asker = GetAsker(quest);
			ResolveParameters(slate, out var requiredPawnCount, out var population, out var colonyMap);
			TryFindSiteTile(out var tile);
			slate.Set("asker", asker);
			slate.Set("askerFaction", asker.Faction);
			slate.Set("requiredPawnCount", requiredPawnCount);
			slate.Set("map", colonyMap);
			Site site = GenerateSite(asker, num, requiredPawnCount, population, tile);
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("askerFaction.BecameHostileToPlayer");
			string text2 = QuestGenUtility.QuestTagSignal(text, "AllEnemiesDefeated");
			string signalSentSatisfied = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.SentSatisfied");
			QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Spawned");
			string text3 = QuestGenUtility.QuestTagSignal(text, "MapRemoved");
			string signalChosenPawn = QuestGen.GenerateNewSignal("ChosenPawnSignal");
			quest.GiveRewards(new RewardsGeneratorParams
			{
				allowGoodwill = true,
				allowRoyalFavor = true,
				giverFaction = asker.Faction,
				rewardValue = RewardValueCurve.Evaluate(num),
				chosenPawnSignal = signalChosenPawn
			}, text2, null, null, null, null, null, delegate
			{
				quest.Letter(LetterDefOf.ChoosePawn, null, label: asker.Faction.def.royalFavorLabel, text: "LetterTextHonorAward_BanditCamp".Translate(asker.Faction.def.royalFavorLabel), chosenPawnSignal: signalChosenPawn, relatedFaction: null, useColonistsOnMap: null, useColonistsFromCaravanArg: false, signalListenMode: QuestPart.SignalListenMode.OngoingOnly, lookTargets: null, filterDeadPawnsFromLookTargets: false, textRules: null, labelRules: null, getColonistsFromSignal: signalSentSatisfied);
			}, null, AddCampLootReward, asker);
			Thing shuttle = QuestGen_Shuttle.GenerateShuttle(null, null, null, acceptColonists: true, onlyAcceptColonists: true, onlyAcceptHealthy: false, requiredPawnCount, dropEverythingIfUnsatisfied: true, leaveImmediatelyWhenSatisfied: true, dropEverythingOnArrival: false, stayAfterDroppedEverythingOnArrival: true, site, colonyMap.Parent, requiredPawnCount, null, permitShuttle: false, hideControls: false);
			shuttle.TryGetComp<CompShuttle>().sendAwayIfQuestFinished = quest;
			slate.Set("shuttle", shuttle);
			quest.SpawnWorldObject(site, null, signalSentSatisfied);
			QuestUtility.AddQuestTag(ref shuttle.questTags, text);
			quest.SpawnSkyfaller(colonyMap, ThingDefOf.ShuttleIncoming, Gen.YieldSingle(shuttle), Faction.OfPlayer, null, null, lookForSafeSpot: true, tryLandInShipLandingZone: true);
			quest.ShuttleLeaveDelay(shuttle, 60000, null, Gen.YieldSingle(signalSentSatisfied), null, delegate
			{
				quest.SendShuttleAway(shuttle, dropEverything: true);
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			});
			string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Killed");
			quest.SetFactionRelations(asker.Faction, FactionRelationKind.Hostile, inSignal2);
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.SignalPass(delegate
			{
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			}, inSignal);
			quest.FeedPawns(null, shuttle, signalSentSatisfied);
			Quest quest2 = quest;
			Action action = delegate
			{
				quest.SendShuttleAway(shuttle, dropEverything: true);
			};
			string inSignalDisable = signalSentSatisfied;
			quest2.SignalPassActivable(action, null, text2, null, null, inSignalDisable);
			QuestUtility.AddQuestTag(ref site.questTags, text);
			slate.Set("site", site);
			quest.SendShuttleAwayOnCleanup(shuttle, dropEverything: true);
			quest.SignalPassActivable(delegate
			{
				quest.Message("MessageMissionGetBackToShuttle".Translate(site.Faction.Named("FACTION")), MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, new LookTargets(shuttle));
			}, signalSentSatisfied, text2);
			quest.SignalPassAllSequence(delegate
			{
				quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			}, new List<string>
			{
				signalSentSatisfied,
				text2,
				text3
			});
			Quest quest3 = quest;
			Action action2 = delegate
			{
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			};
			inSignalDisable = text2;
			quest3.SignalPassActivable(action2, null, text3, null, null, inSignalDisable);
			int num2 = (int)(timeLimitDays.RandomInRange * 60000f);
			slate.Set("timeoutTicks", num2);
			quest.WorldObjectTimeout(site, num2);
			List<Rule> list = new List<Rule>();
			list.AddRange(GrammarUtility.RulesForWorldObject("site", site));
			QuestGen.AddQuestDescriptionRules(list);
		}

		protected override bool TestRunInt(Slate slate)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Missions are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 324345634);
				return false;
			}
			if (IsViolent && !Find.Storyteller.difficultyValues.allowViolentQuests)
			{
				return false;
			}
			ResolveParameters(slate, out var requiredPawnCount, out var population, out var colonyMap);
			if (requiredPawnCount == -1)
			{
				return false;
			}
			if (CanGetAsker() && colonyMap != null)
			{
				return TryFindSiteTile(out population);
			}
			return false;
		}
	}
}
