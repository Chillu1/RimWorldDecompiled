using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public class QuestNode_GiveRewards : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<RewardsGeneratorParams> parms;

		public SlateRef<string> customLetterLabel;

		public SlateRef<string> customLetterText;

		public SlateRef<RulePack> customLetterLabelRules;

		public SlateRef<RulePack> customLetterTextRules;

		public SlateRef<bool?> useDifficultyFactor;

		public QuestNode nodeIfChosenPawnSignalUsed;

		private const float MinRewardValue = 250f;

		protected override bool TestRunInt(Slate slate)
		{
			if (nodeIfChosenPawnSignalUsed != null)
			{
				return nodeIfChosenPawnSignalUsed.TestRun(slate);
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Pawn pawn = slate.Get<Pawn>("asker");
			bool num = useDifficultyFactor.GetValue(slate) ?? true;
			RewardsGeneratorParams value = parms.GetValue(slate);
			value.rewardValue = slate.Get("rewardValue", 0f);
			if (num)
			{
				value.rewardValue *= Find.Storyteller.difficulty.questRewardValueFactor;
			}
			value.minGeneratedRewardValue = 250f;
			value.giverFaction = pawn?.Faction;
			value.populationIntent = QuestTuning.PopIncreasingRewardWeightByPopIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntentForQuest);
			if (value.giverFaction == null || value.giverFaction.def.permanentEnemy)
			{
				value.allowGoodwill = false;
			}
			if (value.giverFaction == null || pawn.royalty == null || !pawn.royalty.HasAnyTitleIn(pawn.Faction))
			{
				value.allowRoyalFavor = false;
			}
			Slate.VarRestoreInfo restoreInfo = slate.GetRestoreInfo("inSignal");
			if (!inSignal.GetValue(slate).NullOrEmpty())
			{
				slate.Set("inSignal", QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)));
			}
			try
			{
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				List<Rule> list3 = new List<Rule>();
				bool flag = false;
				bool flag2 = false;
				List<Reward> list4 = RewardsGenerator.Generate(value);
				list4.SortBy((Reward x) => x is Reward_Items);
				for (int i = 0; i < list4.Count; i++)
				{
					list.Add(list4[i].GetDescription(value));
					if (!(list4[i] is Reward_Items))
					{
						list2.Add(list4[i].GetDescription(value));
					}
					else if (i == list4.Count - 1)
					{
						flag2 = true;
					}
					list4[i].AddQuestPartsToGeneratingQuest(i, value, customLetterLabel.GetValue(slate), customLetterText.GetValue(slate), customLetterLabelRules.GetValue(slate), customLetterTextRules.GetValue(slate));
					if (!parms.GetValue(slate).chosenPawnSignal.NullOrEmpty() && list4[i].MakesUseOfChosenPawnSignal)
					{
						flag = true;
					}
				}
				string text = list.AsEnumerable().ToList().ToClauseSequence()
					.Resolve()
					.UncapitalizeFirst();
				if (flag2)
				{
					text = text.TrimEnd('.');
				}
				list3.Add(new Rule_String("allRewardsDescriptions", text.UncapitalizeFirst()));
				list3.Add(new Rule_String("allRewardsDescriptionsExceptItems", list2.Any() ? list2.AsEnumerable().ToList().ToClauseSequence()
					.Resolve()
					.UncapitalizeFirst() : ""));
				QuestGen.AddQuestDescriptionRules(list3);
				if (flag && nodeIfChosenPawnSignalUsed != null)
				{
					nodeIfChosenPawnSignalUsed.Run();
				}
			}
			finally
			{
				slate.Restore(restoreInfo);
			}
		}
	}
}
