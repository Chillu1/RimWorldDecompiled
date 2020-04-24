using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Reward_Goodwill : Reward
	{
		public int amount;

		public Faction faction;

		public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
		{
			amount = GenMath.RoundRandom(RewardsGenerator.RewardValueToGoodwillCurve.Evaluate(rewardValue));
			amount = Mathf.Min(amount, 100 - parms.giverFaction.PlayerGoodwill);
			amount = Mathf.Max(amount, 1);
			valueActuallyUsed = RewardsGenerator.RewardValueToGoodwillCurve.EvaluateInverted(amount);
			faction = parms.giverFaction;
		}

		public override void AddQuestPartsToGeneratingQuest(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
		{
			QuestPart_FactionGoodwillChange questPart_FactionGoodwillChange = new QuestPart_FactionGoodwillChange();
			questPart_FactionGoodwillChange.change = amount;
			questPart_FactionGoodwillChange.faction = faction;
			questPart_FactionGoodwillChange.inSignal = RimWorld.QuestGen.QuestGen.slate.Get<string>("inSignal");
			RimWorld.QuestGen.QuestGen.quest.AddPart(questPart_FactionGoodwillChange);
		}

		public override string GetDescription(RewardsGeneratorParams parms)
		{
			return "Reward_Goodwill".Translate(faction, amount).Resolve();
		}

		public override string ToString()
		{
			return GetType().Name + " (faction=" + faction.ToStringSafe() + ", amount=" + amount + ")";
		}
	}
}
