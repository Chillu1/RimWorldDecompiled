using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Reward_RoyalFavor : Reward
	{
		public int amount;

		public Faction faction;

		public override bool MakesUseOfChosenPawnSignal => true;

		public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
		{
			amount = GenMath.RoundRandom(RewardsGenerator.RewardValueToRoyalFavorCurve.Evaluate(rewardValue));
			amount = Mathf.Clamp(amount, 1, 12);
			valueActuallyUsed = RewardsGenerator.RewardValueToRoyalFavorCurve.EvaluateInverted(amount);
			faction = parms.giverFaction;
		}

		public override void AddQuestPartsToGeneratingQuest(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
		{
			Slate slate = RimWorld.QuestGen.QuestGen.slate;
			QuestPart_GiveRoyalFavor questPart_GiveRoyalFavor = new QuestPart_GiveRoyalFavor();
			questPart_GiveRoyalFavor.faction = faction;
			questPart_GiveRoyalFavor.amount = amount;
			if (!parms.chosenPawnSignal.NullOrEmpty())
			{
				questPart_GiveRoyalFavor.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(parms.chosenPawnSignal);
				questPart_GiveRoyalFavor.signalListenMode = QuestPart.SignalListenMode.Always;
			}
			else
			{
				questPart_GiveRoyalFavor.inSignal = slate.Get<string>("inSignal");
				questPart_GiveRoyalFavor.giveToAccepter = true;
			}
			RimWorld.QuestGen.QuestGen.quest.AddPart(questPart_GiveRoyalFavor);
			slate.Set("royalFavorReward_amount", amount);
		}

		public override string GetDescription(RewardsGeneratorParams parms)
		{
			if (!parms.chosenPawnSignal.NullOrEmpty())
			{
				return "Reward_RoyalFavor_ChoosePawn".Translate(faction, amount, Faction.OfPlayer.def.pawnsPlural).Resolve();
			}
			return "Reward_RoyalFavor".Translate(faction, amount).Resolve();
		}

		public override string ToString()
		{
			return GetType().Name + " (faction=" + faction.ToStringSafe() + ", amount=" + amount + ")";
		}
	}
}
