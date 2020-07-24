using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

		public SlateRef<int?> variants;

		public SlateRef<bool> addCampLootReward;

		private List<List<Reward>> generatedRewards = new List<List<Reward>>();

		private const float MinRewardValue = 250f;

		private const int DefaultVariants = 3;

		private static List<QuestPart> tmpPrevQuestParts = new List<QuestPart>();

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
			if (slate.Get("debugDontGenerateRewardThings", defaultValue: false))
			{
				DebugActionsQuests.lastQuestGeneratedRewardValue += Mathf.Max(value.rewardValue, 250f);
				return;
			}
			value.minGeneratedRewardValue = 250f;
			value.giverFaction = pawn?.Faction;
			value.populationIntent = QuestTuning.PopIncreasingRewardWeightByPopIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntentForQuest);
			if (value.giverFaction == null || value.giverFaction.def.permanentEnemy)
			{
				value.allowGoodwill = false;
			}
			if (value.giverFaction == null || pawn.royalty == null || !pawn.royalty.HasAnyTitleIn(pawn.Faction) || value.giverFaction.HostileTo(Faction.OfPlayer))
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
				QuestPart_Choice questPart_Choice = new QuestPart_Choice();
				questPart_Choice.inSignalChoiceUsed = slate.Get<string>("inSignal");
				bool chosenPawnSignalUsed = false;
				int num2 = (value.allowGoodwill && value.giverFaction != null && value.giverFaction.HostileTo(Faction.OfPlayer)) ? 1 : (variants.GetValue(slate) ?? (QuestGen.quest.root.autoAccept ? 1 : 3));
				generatedRewards.Clear();
				for (int i = 0; i < num2; i++)
				{
					QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
					List<Reward> list = GenerateRewards(value, slate, i == 0, ref chosenPawnSignalUsed, choice, num2);
					if (list != null)
					{
						questPart_Choice.choices.Add(choice);
						generatedRewards.Add(list);
					}
				}
				generatedRewards.Clear();
				if (addCampLootReward.GetValue(slate))
				{
					for (int j = 0; j < questPart_Choice.choices.Count; j++)
					{
						questPart_Choice.choices[j].rewards.Add(new Reward_CampLoot());
					}
				}
				questPart_Choice.choices.SortByDescending(GetDisplayPriority);
				QuestGen.quest.AddPart(questPart_Choice);
				if (!chosenPawnSignalUsed || nodeIfChosenPawnSignalUsed == null)
				{
					return;
				}
				tmpPrevQuestParts.Clear();
				tmpPrevQuestParts.AddRange(QuestGen.quest.PartsListForReading);
				nodeIfChosenPawnSignalUsed.Run();
				List<QuestPart> partsListForReading = QuestGen.quest.PartsListForReading;
				for (int k = 0; k < partsListForReading.Count; k++)
				{
					if (tmpPrevQuestParts.Contains(partsListForReading[k]))
					{
						continue;
					}
					for (int l = 0; l < questPart_Choice.choices.Count; l++)
					{
						bool flag = false;
						for (int m = 0; m < questPart_Choice.choices[l].rewards.Count; m++)
						{
							if (questPart_Choice.choices[l].rewards[m].MakesUseOfChosenPawnSignal)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							questPart_Choice.choices[l].questParts.Add(partsListForReading[k]);
						}
					}
				}
				tmpPrevQuestParts.Clear();
			}
			finally
			{
				slate.Restore(restoreInfo);
			}
		}

		private List<Reward> GenerateRewards(RewardsGeneratorParams parmsResolved, Slate slate, bool addDescriptionRules, ref bool chosenPawnSignalUsed, QuestPart_Choice.Choice choice, int variants)
		{
			List<string> list;
			List<string> list2;
			if (addDescriptionRules)
			{
				list = new List<string>();
				list2 = new List<string>();
			}
			else
			{
				list = null;
				list2 = null;
			}
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < generatedRewards.Count; i++)
			{
				for (int j = 0; j < generatedRewards[i].Count; j++)
				{
					if (generatedRewards[i][j] is Reward_Pawn)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
			}
			if (flag2)
			{
				parmsResolved.thingRewardItemsOnly = true;
			}
			List<Reward> list3 = null;
			if (variants >= 2 && !generatedRewards.Any() && parmsResolved.allowGoodwill && !parmsResolved.thingRewardRequired)
			{
				list3 = TryGenerateRewards_SocialOnly(parmsResolved, variants >= 3);
				if (list3.NullOrEmpty() && variants >= 3)
				{
					list3 = TryGenerateRewards_ThingsOnly(parmsResolved);
				}
				if (list3.NullOrEmpty())
				{
					list3 = TryGenerateNonRepeatingRewards(parmsResolved);
				}
			}
			else if (variants >= 3 && generatedRewards.Count == 1 && parmsResolved.allowRoyalFavor && !parmsResolved.thingRewardRequired)
			{
				list3 = TryGenerateRewards_RoyalFavorOnly(parmsResolved);
				if (list3.NullOrEmpty())
				{
					list3 = TryGenerateRewards_ThingsOnly(parmsResolved);
				}
				if (list3.NullOrEmpty())
				{
					list3 = TryGenerateNonRepeatingRewards(parmsResolved);
				}
			}
			else if (variants >= 2 && generatedRewards.Any() && !parmsResolved.thingRewardDisallowed)
			{
				list3 = TryGenerateRewards_ThingsOnly(parmsResolved);
				if (list3.NullOrEmpty())
				{
					list3 = TryGenerateNonRepeatingRewards(parmsResolved);
				}
			}
			else
			{
				list3 = TryGenerateNonRepeatingRewards(parmsResolved);
			}
			if (list3.NullOrEmpty())
			{
				return null;
			}
			Reward_Items reward_Items = (Reward_Items)list3.Find((Reward x) => x is Reward_Items);
			if (reward_Items == null)
			{
				List<Type> b = list3.Select((Reward x) => x.GetType()).ToList();
				for (int k = 0; k < generatedRewards.Count; k++)
				{
					if (generatedRewards[k].Select((Reward x) => x.GetType()).ToList().ListsEqualIgnoreOrder(b))
					{
						return null;
					}
				}
			}
			else if (list3.Count == 1)
			{
				List<ThingDef> b2 = reward_Items.ItemsListForReading.Select((Thing x) => x.def).ToList();
				for (int l = 0; l < generatedRewards.Count; l++)
				{
					Reward_Items reward_Items2 = (Reward_Items)generatedRewards[l].Find((Reward x) => x is Reward_Items);
					if (reward_Items2 != null && reward_Items2.ItemsListForReading.Select((Thing x) => x.def).ToList().ListsEqualIgnoreOrder(b2))
					{
						return null;
					}
				}
			}
			list3.SortBy((Reward x) => x is Reward_Items);
			choice.rewards.AddRange(list3);
			for (int m = 0; m < list3.Count; m++)
			{
				if (addDescriptionRules)
				{
					list.Add(list3[m].GetDescription(parmsResolved));
					if (!(list3[m] is Reward_Items))
					{
						list2.Add(list3[m].GetDescription(parmsResolved));
					}
					else if (m == list3.Count - 1)
					{
						flag = true;
					}
				}
				foreach (QuestPart item in list3[m].GenerateQuestParts(m, parmsResolved, customLetterLabel.GetValue(slate), customLetterText.GetValue(slate), customLetterLabelRules.GetValue(slate), customLetterTextRules.GetValue(slate)))
				{
					QuestGen.quest.AddPart(item);
					choice.questParts.Add(item);
				}
				if (!parms.GetValue(slate).chosenPawnSignal.NullOrEmpty() && list3[m].MakesUseOfChosenPawnSignal)
				{
					chosenPawnSignalUsed = true;
				}
			}
			if (addDescriptionRules)
			{
				string text = list.AsEnumerable().ToList().ToClauseSequence()
					.Resolve()
					.UncapitalizeFirst();
				if (flag)
				{
					text = text.TrimEnd('.');
				}
				QuestGen.AddQuestDescriptionRules(new List<Rule>
				{
					new Rule_String("allRewardsDescriptions", text.UncapitalizeFirst()),
					new Rule_String("allRewardsDescriptionsExceptItems", list2.Any() ? list2.AsEnumerable().ToList().ToClauseSequence()
						.Resolve()
						.UncapitalizeFirst() : "")
				});
			}
			return list3;
		}

		private List<Reward> TryGenerateRewards_SocialOnly(RewardsGeneratorParams parms, bool disallowRoyalFavor)
		{
			parms.thingRewardDisallowed = true;
			if (disallowRoyalFavor)
			{
				parms.allowRoyalFavor = false;
			}
			return TryGenerateNonRepeatingRewards(parms);
		}

		private List<Reward> TryGenerateRewards_RoyalFavorOnly(RewardsGeneratorParams parms)
		{
			parms.allowGoodwill = false;
			parms.thingRewardDisallowed = true;
			return TryGenerateNonRepeatingRewards(parms);
		}

		private List<Reward> TryGenerateRewards_ThingsOnly(RewardsGeneratorParams parms)
		{
			if (parms.thingRewardDisallowed)
			{
				return null;
			}
			parms.allowGoodwill = false;
			parms.allowRoyalFavor = false;
			return TryGenerateNonRepeatingRewards(parms);
		}

		private List<Reward> TryGenerateNonRepeatingRewards(RewardsGeneratorParams parms)
		{
			List<Reward> list = null;
			int num = 0;
			while (num < 10)
			{
				list = RewardsGenerator.Generate(parms);
				if (list.Any((Reward x) => x is Reward_Pawn))
				{
					return list;
				}
				Reward_Items reward_Items = (Reward_Items)list.FirstOrDefault((Reward x) => x is Reward_Items);
				if (reward_Items != null)
				{
					bool flag = false;
					for (int j = 0; j < generatedRewards.Count; j++)
					{
						Reward_Items otherGeneratedItems = null;
						for (int k = 0; k < generatedRewards[j].Count; k++)
						{
							otherGeneratedItems = (generatedRewards[j][k] as Reward_Items);
							if (otherGeneratedItems != null)
							{
								break;
							}
						}
						if (otherGeneratedItems != null)
						{
							int i;
							for (i = 0; i < otherGeneratedItems.items.Count; i++)
							{
								if (reward_Items.items.Any((Thing x) => x.GetInnerIfMinified().def == otherGeneratedItems.items[i].GetInnerIfMinified().def))
								{
									flag = true;
									break;
								}
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						num++;
						continue;
					}
				}
				return list;
			}
			return list;
		}

		private int GetDisplayPriority(QuestPart_Choice.Choice choice)
		{
			for (int i = 0; i < choice.rewards.Count; i++)
			{
				if (choice.rewards[i] is Reward_RoyalFavor)
				{
					return 1;
				}
			}
			for (int j = 0; j < choice.rewards.Count; j++)
			{
				if (choice.rewards[j] is Reward_Goodwill)
				{
					return -1;
				}
			}
			return 0;
		}
	}
}
