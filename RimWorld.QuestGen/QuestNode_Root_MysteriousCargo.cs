using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public abstract class QuestNode_Root_MysteriousCargo : QuestNode
	{
		private static readonly FloatRange DelayHours = new FloatRange(1f, 3f);

		private static readonly SimpleCurve RewardValueCurve = new SimpleCurve
		{
			new CurvePoint(500f, 800f),
			new CurvePoint(3000f, 1200f)
		};

		protected virtual bool RequiresPawn { get; } = true;

		protected override bool TestRunInt(Slate slate)
		{
			Map map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
			if (map == null)
			{
				return false;
			}
			Pawn pawn;
			if (RequiresPawn)
			{
				return QuestUtility.TryGetIdealColonist(out pawn, map, ValidatePawn);
			}
			return true;
		}

		protected override void RunInt()
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
			float points = slate.Get("points", 0f);
			slate.Set("map", map);
			Pawn asker = FindAsker();
			Pawn pawn = null;
			if (RequiresPawn && !QuestUtility.TryGetIdealColonist(out pawn, map, ValidatePawn))
			{
				Log.ErrorOnce("Attempted to create a mysterious cargo quest but no valid pawns or world pawns could be found", 94657346);
				quest.End(QuestEndOutcome.InvalidPreAcceptance);
			}
			Thing thing = GenerateThing(pawn);
			quest.Delay(120, delegate
			{
				quest.DropPods(map.Parent, new List<Thing> { thing }, "[deliveredLetterLabel]", null, "[deliveredLetterText]", null, true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.OngoingOnly, null, destroyItemsOnCleanup: true, dropAllInSamePod: true, allowFogged: false, canRetargetAnyMap: true);
				AddPostDroppedQuestParts(pawn, thing, quest);
			});
			quest.Delay(Mathf.RoundToInt(DelayHours.RandomInRange * 2500f), delegate
			{
				Quest quest2 = quest;
				RewardsGeneratorParams parms = new RewardsGeneratorParams
				{
					rewardValue = RewardValueCurve.Evaluate(points),
					thingRewardItemsOnly = true
				};
				Pawn asker2 = asker;
				quest2.GiveRewards(parms, null, null, null, null, null, null, null, null, addCampLootReward: false, asker2);
				quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			});
			slate.Set("asker", asker);
			slate.Set("askerIsNull", asker == null);
			slate.Set("pawn", pawn);
			slate.Set("pawnOnMap", pawn?.MapHeld == map);
		}

		protected abstract Thing GenerateThing(Pawn pawn);

		protected virtual void AddPostDroppedQuestParts(Pawn pawn, Thing thing, Quest quest)
		{
		}

		protected virtual bool ValidatePawn(Pawn pawn)
		{
			if (!pawn.IsColonist)
			{
				return pawn.IsSlaveOfColony;
			}
			return true;
		}

		private Pawn FindAsker()
		{
			if (Find.FactionManager.AllFactionsVisible.Where((Faction f) => f.def.humanlikeFaction && !f.IsPlayer && !f.HostileTo(Faction.OfPlayer) && (int)f.def.techLevel > 2 && f.leader != null && !f.temporary && !f.Hidden).TryRandomElement(out var result))
			{
				return result.leader;
			}
			return null;
		}
	}
}
