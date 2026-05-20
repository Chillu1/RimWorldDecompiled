using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Root_AncientSignalActivation : QuestNode
	{
		private const int DropPodDelayTicks = 180;

		private static int RaidDelayTicks = 60 * Rand.Range(5, 10);

		private static FloatRange RaidRewardPoints = new FloatRange(150f, 200f);

		private static float RaidChance = 0.5f;

		protected override void RunInt()
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = slate.Get<Map>("map") ?? QuestGen_Get.GetMap();
			IntVec3? dropSpot = null;
			LookTargets lookTargets = null;
			if (TryFindRandomDropSpot(map, out var result))
			{
				lookTargets = new LookTargets(result, map);
				dropSpot = result;
			}
			Quest quest2 = quest;
			string message = "MessageAncientSignalActivated".Translate();
			MessageTypeDef negativeEvent = MessageTypeDefOf.NegativeEvent;
			string initiateSignal = quest.InitiateSignal;
			quest2.Message(message, negativeEvent, getLookTargetsFromSignal: false, null, lookTargets, initiateSignal);
			ThingSetMakerParams parms = new ThingSetMakerParams
			{
				qualityGenerator = QualityGenerator.Reward,
				makingFaction = Faction.OfAncients
			};
			List<Thing> rewards = ThingSetMakerDefOf.ResourcePod.root.Generate(parms);
			quest.Delay(180, delegate
			{
				quest.DropPods(map.Parent, rewards, null, null, null, null, dropSpot: dropSpot, sendStandardLetter: true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, inSignal: null, thingsToExcludeFromHyperlinks: null, signalListenMode: QuestPart.SignalListenMode.OngoingOnly, destroyItemsOnCleanup: true, dropAllInSamePod: false, allowFogged: false, canRetargetAnyMap: true);
				if (Rand.Chance(RaidChance) && Find.Storyteller.difficulty.allowViolentQuests && TryFindRandomEnemyFaction(out var faction))
				{
					quest.Delay(RaidDelayTicks, delegate
					{
						Quest quest3 = quest;
						MapParent parent = map.Parent;
						float min = RaidRewardPoints.min;
						Faction faction2 = faction;
						string customLetterText = "MessageAncientSignalHostileDetected".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction);
						PawnsArrivalModeDef edgeWalkIn = PawnsArrivalModeDefOf.EdgeWalkIn;
						RaidStrategyDef immediateAttack = RaidStrategyDefOf.ImmediateAttack;
						quest3.Raid(parent, min, faction2, null, null, customLetterText, null, null, null, null, null, "root", edgeWalkIn, immediateAttack);
						QuestGen_End.End(quest, QuestEndOutcome.Unknown);
					});
				}
				else
				{
					QuestGen_End.End(quest, QuestEndOutcome.Unknown);
				}
			}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, "RewardDelay");
		}

		private bool TryFindRandomEnemyFaction(out Faction faction)
		{
			faction = Find.FactionManager.RandomRaidableEnemyFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: true, TechLevel.Industrial);
			return faction != null;
		}

		private bool TryFindRandomDropSpot(Map map, out IntVec3 result)
		{
			return CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => x.Standable(map) && !x.Roofed(map) && !x.Fogged(map), map, 1000, out result);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return ModLister.CheckIdeology("Ancient signal activation quest");
		}
	}
}
