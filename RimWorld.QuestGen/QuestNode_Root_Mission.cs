using System;
using System.Collections.Generic;
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

		public bool canBeSpace;

		[Obsolete("This field is no longer used.")]
		private static readonly List<Map> tmpMaps = new List<Map>();

		protected abstract string QuestTag { get; }

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

		protected abstract Site GenerateSite(Pawn asker, float threatPoints, int pawnCount, int population, PlanetTile tile);

		protected virtual bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
		{
			return TileFinder.TryFindNewSiteTile(out tile, 80, 85, allowCaravans: false, null, 0.5f, canSelectComboLandmarks: true, TileFinderMode.Near, exitOnFirstTileFound, canBeSpace);
		}

		public static bool PawnCanFight(Pawn p)
		{
			if (p.Downed)
			{
				return false;
			}
			if (p.health.hediffSet.BleedRateTotal > 0f)
			{
				return false;
			}
			if (p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff())
			{
				return false;
			}
			if (p.IsQuestLodger())
			{
				return false;
			}
			if (p.IsSlave)
			{
				return false;
			}
			if (!p.DevelopmentalStage.Adult())
			{
				return false;
			}
			return true;
		}

		private void ResolveParameters(Slate slate, out int requiredPawnCount, out int population, out Map colonyMap)
		{
			colonyMap = slate.Get<Map>("map");
			if (colonyMap != null && colonyMap.IsPlayerHome)
			{
				QuestScriptDef root = QuestGen.Root;
				if (root == null || root.IsParentSuitableForQuest(colonyMap.Parent))
				{
					goto IL_0066;
				}
			}
			colonyMap = QuestGen.Root?.TryFindNewSuitableMapParentForRetarget()?.Map ?? Find.AnyPlayerHomeMap;
			goto IL_0066;
			IL_0066:
			if (colonyMap == null)
			{
				population = -1;
				requiredPawnCount = -1;
			}
			else
			{
				population = (slate.Exists("population") ? slate.Get("population", 0) : colonyMap.mapPawns.FreeColonists.Count(DoesPawnCountAsAvailableForFight));
				requiredPawnCount = GetRequiredPawnCount(population, slate.Get("points", 0));
			}
		}

		protected override void RunInt()
		{
			if (!ModLister.CheckRoyalty("Mission"))
			{
				return;
			}
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(QuestTag);
			QuestGenUtility.RunAdjustPointsForDistantFight();
			int num = slate.Get("points", 0);
			Pawn asker = GetAsker(quest);
			ResolveParameters(slate, out var requiredPawnCount, out var population, out var colonyMap);
			if (requiredPawnCount <= 0 || population <= 0)
			{
				Log.Error($"Mission '{text}' of type '{GetType().Name}' and def '{QuestGen.Root.defName}' has invalid required pawn count ({requiredPawnCount}) or population ({population}). This should have been caught in TestRunInt().");
				return;
			}
			TryFindSiteTile(out var tile);
			slate.Set("asker", asker);
			slate.Set("askerFaction", asker.Faction);
			slate.Set("requiredPawnCount", requiredPawnCount);
			slate.Set("map", colonyMap);
			Site site = GenerateSite(asker, num, requiredPawnCount, population, tile);
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("askerFaction.BecameHostileToPlayer");
			string text2 = QuestGenUtility.QuestTagSignal(text, "AllEnemiesDefeated");
			string signalSentSatisfied = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.SentSatisfied");
			string text3 = QuestGenUtility.QuestTagSignal(text, "MapRemoved");
			string signalChosenPawn = QuestGen.GenerateNewSignal("ChosenPawnSignal");
			quest.GiveRewards(new RewardsGeneratorParams
			{
				allowGoodwill = true,
				allowRoyalFavor = true,
				giverFaction = asker.Faction,
				rewardValue = RewardValueCurve.Evaluate(num),
				chosenPawnSignal = signalChosenPawn
			}, text2, null, null, null, null, addCampLootReward: AddCampLootReward, asker: asker, useDifficultyFactor: null, runIfChosenPawnSignalUsed: delegate
			{
				quest.Letter(LetterDefOf.ChoosePawn, null, label: asker.Faction.def.royalFavorLabel, text: "LetterTextHonorAward_BanditCamp".Translate(asker.Faction.def.royalFavorLabel), chosenPawnSignal: signalChosenPawn, relatedFaction: null, useColonistsOnMap: null, useColonistsFromCaravanArg: false, signalListenMode: QuestPart.SignalListenMode.OngoingOnly, lookTargets: null, filterDeadPawnsFromLookTargets: false, textRules: null, labelRules: null, getColonistsFromSignal: signalSentSatisfied);
			});
			Thing shuttle = QuestGen_Shuttle.GenerateShuttle(null, null, null, acceptColonists: true, onlyAcceptColonists: true, onlyAcceptHealthy: false, requiredPawnCount, dropEverythingIfUnsatisfied: true, leaveImmediatelyWhenSatisfied: true, dropEverythingOnArrival: false, stayAfterDroppedEverythingOnArrival: true, site, colonyMap.Parent, requiredPawnCount, null, permitShuttle: false, hideControls: false, allowSlaves: false, requireAllColonistsOnMap: false, acceptColonyPrisoners: true);
			slate.Set("shuttle", shuttle);
			QuestUtility.AddQuestTag(ref shuttle.questTags, text);
			quest.SpawnWorldObject(site);
			TransportShip transportShip = quest.GenerateTransportShip(TransportShipDefOf.Ship_Shuttle, null, shuttle).transportShip;
			slate.Set("transportShip", transportShip);
			QuestUtility.AddQuestTag(ref transportShip.questTags, text);
			quest.SendTransportShipAwayOnCleanup(transportShip, unloadContents: true, TransportShipDropMode.None);
			Quest quest2 = quest;
			MapParent parent = colonyMap.Parent;
			Faction ofEmpire = Faction.OfEmpire;
			quest2.AddShipJob_Arrive(transportShip, parent, null, null, ShipJobStartMode.Instant, ofEmpire);
			quest.AddShipJob_WaitSendable(transportShip, site, leaveImmeiatelyWhenSatisfied: true);
			quest.AddShipJob(transportShip, ShipJobDefOf.Unload);
			quest.AddShipJob_WaitSendable(transportShip, colonyMap.Parent, leaveImmeiatelyWhenSatisfied: true, targetPlayerSettlement: true);
			quest.AddShipJob(transportShip, ShipJobDefOf.Unload);
			quest.AddShipJob_FlyAway(transportShip, null, null, TransportShipDropMode.None);
			quest.TendPawns(null, shuttle, signalSentSatisfied);
			quest.RequiredShuttleThings(shuttle, site, QuestGenUtility.HardcodedSignalWithQuestID("transportShip.FlewAway"), requireAllColonistsOnMap: true);
			quest.ShuttleLeaveDelay(shuttle, 60000, null, Gen.YieldSingle(signalSentSatisfied), null, delegate
			{
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			});
			string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Killed");
			quest.FactionGoodwillChange(asker.Faction, 0, inSignal2, canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.ShuttleDestroyed, QuestPart.SignalListenMode.OngoingOnly, ensureMakesHostile: true);
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			string inSignal3 = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.LeftBehind");
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal3, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.SignalPass(delegate
			{
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			}, inSignal);
			quest.FeedPawns(null, shuttle, signalSentSatisfied);
			QuestUtility.AddQuestTag(ref site.questTags, text);
			slate.Set("site", site);
			quest.SignalPassActivable(delegate
			{
				quest.Message("MessageMissionGetBackToShuttle".Translate(site.Faction.Named("FACTION")), MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, new LookTargets(shuttle));
				quest.Notify_PlayerRaidedSomeone(null, site);
			}, signalSentSatisfied, text2);
			quest.SignalPassAllSequence(delegate
			{
				quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			}, new List<string> { signalSentSatisfied, text2, text3 });
			Quest quest3 = quest;
			Action action = delegate
			{
				quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			};
			string inSignalDisable = text2;
			quest3.SignalPassActivable(action, null, text3, null, null, inSignalDisable);
			int num2 = (int)(timeLimitDays.RandomInRange * 60000f);
			slate.Set("timeoutTicks", num2);
			quest.WorldObjectTimeout(site, num2);
			List<Rule> list = new List<Rule>();
			list.AddRange(GrammarUtility.RulesForWorldObject("site", site));
			QuestGen.AddQuestDescriptionRules(list);
		}

		protected override bool TestRunInt(Slate slate)
		{
			QuestGenUtility.TestRunAdjustPointsForDistantFight(slate);
			if (!ModLister.CheckRoyalty("Mission"))
			{
				return false;
			}
			if (IsViolent && !Find.Storyteller.difficulty.allowViolentQuests)
			{
				return false;
			}
			ResolveParameters(slate, out var requiredPawnCount, out var population, out var colonyMap);
			if (requiredPawnCount <= 0 || population <= 0)
			{
				return false;
			}
			PlanetTile tile;
			if (CanGetAsker() && colonyMap != null)
			{
				return TryFindSiteTile(out tile, exitOnFirstTileFound: true);
			}
			return false;
		}
	}
}
