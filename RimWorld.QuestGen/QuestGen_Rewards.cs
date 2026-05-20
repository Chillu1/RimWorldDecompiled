using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public static class QuestGen_Rewards
{
	private const float MinRewardValue = 250f;

	private const int DefaultVariants = 3;

	private static List<List<Reward>> generatedRewards = new List<List<Reward>>();

	private static List<QuestPart> tmpPrevQuestParts = new List<QuestPart>();

	public static QuestPart_Choice GiveRewards(this Quest quest, RewardsGeneratorParams parms, string inSignal = null, string customLetterLabel = null, string customLetterText = null, RulePack customLetterLabelRules = null, RulePack customLetterTextRules = null, bool? useDifficultyFactor = null, Action runIfChosenPawnSignalUsed = null, int? variants = null, bool addCampLootReward = false, Pawn asker = null, bool addShuttleLootReward = false, bool addPossibleFutureReward = false, float? overridePopulationIntent = null)
	{
		try
		{
			Slate slate = QuestGen.slate;
			RewardsGeneratorParams parmsResolved = parms;
			parmsResolved.rewardValue = ((parmsResolved.rewardValue == 0f) ? slate.Get("rewardValue", 0f) : parmsResolved.rewardValue);
			if (useDifficultyFactor ?? true)
			{
				parmsResolved.rewardValue *= Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
				parmsResolved.rewardValue = Math.Max(1f, parmsResolved.rewardValue);
			}
			if (slate.Get("debugDontGenerateRewardThings", defaultValue: false))
			{
				DebugActionsQuests.lastQuestGeneratedRewardValue += Mathf.Max(parmsResolved.rewardValue, 250f);
				return null;
			}
			parmsResolved.minGeneratedRewardValue = 250f;
			parmsResolved.giverFaction = parmsResolved.giverFaction ?? asker?.Faction;
			parmsResolved.populationIntent = overridePopulationIntent ?? QuestTuning.PopIncreasingRewardWeightByPopIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntentForQuest);
			if (parmsResolved.giverFaction == null || parmsResolved.giverFaction.def.permanentEnemy)
			{
				parmsResolved.allowGoodwill = false;
			}
			if (parmsResolved.giverFaction == null || asker.royalty == null || !asker.royalty.HasAnyTitleIn(asker.Faction) || parmsResolved.giverFaction.HostileTo(Faction.OfPlayer))
			{
				parmsResolved.allowRoyalFavor = false;
			}
			if (!ModsConfig.IdeologyActive || Faction.OfPlayer.ideos.FluidIdeo == null)
			{
				parmsResolved.allowDevelopmentPoints = false;
			}
			if (!ModsConfig.BiotechActive)
			{
				parmsResolved.allowXenogermReimplantation = false;
			}
			Slate.VarRestoreInfo restoreInfo = slate.GetRestoreInfo("inSignal");
			if (inSignal.NullOrEmpty())
			{
				inSignal = slate.Get<string>("inSignal");
			}
			else
			{
				slate.Set("inSignal", QuestGenUtility.HardcodedSignalWithQuestID(inSignal));
			}
			try
			{
				QuestPart_Choice questPart_Choice = new QuestPart_Choice();
				questPart_Choice.inSignalChoiceUsed = slate.Get<string>("inSignal");
				bool chosenPawnSignalUsed = false;
				int num = ((parmsResolved.allowGoodwill && parmsResolved.giverFaction != null && parmsResolved.giverFaction.HostileTo(Faction.OfPlayer)) ? 1 : (variants ?? (QuestGen.quest.root.autoAccept ? 1 : 3)));
				generatedRewards.Clear();
				for (int i = 0; i < num; i++)
				{
					QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
					List<Reward> list = GenerateRewards(parmsResolved, slate, i == 0, ref chosenPawnSignalUsed, choice, num, customLetterLabel, customLetterText, customLetterLabelRules, customLetterTextRules);
					if (list != null)
					{
						questPart_Choice.choices.Add(choice);
						generatedRewards.Add(list);
					}
				}
				generatedRewards.Clear();
				if (addCampLootReward)
				{
					for (int j = 0; j < questPart_Choice.choices.Count; j++)
					{
						questPart_Choice.choices[j].rewards.Add(new Reward_CampLoot());
					}
				}
				if (addShuttleLootReward)
				{
					for (int k = 0; k < questPart_Choice.choices.Count; k++)
					{
						questPart_Choice.choices[k].rewards.Add(new Reward_ShuttleLoot());
					}
				}
				if (addPossibleFutureReward)
				{
					for (int l = 0; l < questPart_Choice.choices.Count; l++)
					{
						questPart_Choice.choices[l].rewards.Add(new Reward_PossibleFutureReward());
					}
				}
				if (parmsResolved.allowDevelopmentPoints)
				{
					if (questPart_Choice.choices.Count <= 0)
					{
						QuestPart_Choice.Choice choice2 = new QuestPart_Choice.Choice();
						choice2.rewards.Add(new Reward_DevelopmentPoints(quest));
						questPart_Choice.choices.Add(choice2);
					}
					else
					{
						for (int m = 0; m < questPart_Choice.choices.Count; m++)
						{
							questPart_Choice.choices[m].rewards.Add(new Reward_DevelopmentPoints(quest));
						}
					}
				}
				if (parmsResolved.allowXenogermReimplantation)
				{
					QuestPart_Choice.Choice choice3 = new QuestPart_Choice.Choice();
					Reward_ReimplantXenogerm reward_ReimplantXenogerm = new Reward_ReimplantXenogerm();
					choice3.rewards.Add(reward_ReimplantXenogerm);
					questPart_Choice.choices.Add(choice3);
					foreach (QuestPart item in reward_ReimplantXenogerm.GenerateQuestParts(0, parms, null, null, null, null))
					{
						QuestGen.quest.AddPart(item);
						choice3.questParts.Add(item);
					}
				}
				questPart_Choice.choices.SortByDescending(GetDisplayPriority);
				QuestGen.quest.AddPart(questPart_Choice);
				if (chosenPawnSignalUsed && runIfChosenPawnSignalUsed != null)
				{
					tmpPrevQuestParts.Clear();
					tmpPrevQuestParts.AddRange(QuestGen.quest.PartsListForReading);
					runIfChosenPawnSignalUsed();
					List<QuestPart> partsListForReading = QuestGen.quest.PartsListForReading;
					for (int n = 0; n < partsListForReading.Count; n++)
					{
						if (tmpPrevQuestParts.Contains(partsListForReading[n]))
						{
							continue;
						}
						for (int num2 = 0; num2 < questPart_Choice.choices.Count; num2++)
						{
							bool flag = false;
							for (int num3 = 0; num3 < questPart_Choice.choices[num2].rewards.Count; num3++)
							{
								if (questPart_Choice.choices[num2].rewards[num3].MakesUseOfChosenPawnSignal)
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								questPart_Choice.choices[num2].questParts.Add(partsListForReading[n]);
							}
						}
					}
					tmpPrevQuestParts.Clear();
				}
				return questPart_Choice;
			}
			finally
			{
				slate.Restore(restoreInfo);
			}
		}
		finally
		{
			generatedRewards.Clear();
		}
	}

	private static List<Reward> GenerateRewards(RewardsGeneratorParams parmsResolved, Slate slate, bool addDescriptionRules, ref bool chosenPawnSignalUsed, QuestPart_Choice.Choice choice, int variants, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
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
			for (int num = 0; num < generatedRewards.Count; num++)
			{
				if (generatedRewards[num].Select((Reward x) => x.GetType()).ToList().SetsEqual(b))
				{
					return null;
				}
			}
		}
		else if (list3.Count == 1)
		{
			List<ThingDef> b2 = reward_Items.ItemsListForReading.Select((Thing x) => x.def).ToList();
			for (int num2 = 0; num2 < generatedRewards.Count; num2++)
			{
				Reward_Items reward_Items2 = (Reward_Items)generatedRewards[num2].Find((Reward x) => x is Reward_Items);
				if (reward_Items2 != null && reward_Items2.ItemsListForReading.Select((Thing x) => x.def).ToList().SetsEqual(b2))
				{
					return null;
				}
			}
		}
		list3.SortBy((Reward x) => x is Reward_Items);
		choice.rewards.AddRange(list3);
		for (int num3 = 0; num3 < list3.Count; num3++)
		{
			if (addDescriptionRules)
			{
				list.Add(list3[num3].GetDescription(parmsResolved));
				if (!(list3[num3] is Reward_Items))
				{
					list2.Add(list3[num3].GetDescription(parmsResolved));
				}
				else if (num3 == list3.Count - 1)
				{
					flag = true;
				}
			}
			foreach (QuestPart item in list3[num3].GenerateQuestParts(num3, parmsResolved, customLetterLabel, customLetterText, customLetterLabelRules, customLetterTextRules))
			{
				QuestGen.quest.AddPart(item);
				choice.questParts.Add(item);
			}
			if (!parmsResolved.chosenPawnSignal.NullOrEmpty() && list3[num3].MakesUseOfChosenPawnSignal)
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

	private static List<Reward> TryGenerateRewards_SocialOnly(RewardsGeneratorParams parms, bool disallowRoyalFavor)
	{
		parms.thingRewardDisallowed = true;
		if (disallowRoyalFavor)
		{
			parms.allowRoyalFavor = false;
		}
		return TryGenerateNonRepeatingRewards(parms);
	}

	private static List<Reward> TryGenerateRewards_RoyalFavorOnly(RewardsGeneratorParams parms)
	{
		parms.allowGoodwill = false;
		parms.thingRewardDisallowed = true;
		return TryGenerateNonRepeatingRewards(parms);
	}

	private static List<Reward> TryGenerateRewards_ThingsOnly(RewardsGeneratorParams parms)
	{
		if (parms.thingRewardDisallowed)
		{
			return null;
		}
		parms.allowGoodwill = false;
		parms.allowRoyalFavor = false;
		return TryGenerateNonRepeatingRewards(parms);
	}

	private static List<Reward> TryGenerateNonRepeatingRewards(RewardsGeneratorParams parms)
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
			if (list.Any((Reward x) => x is Reward_ReimplantXenogerm))
			{
				return list;
			}
			Reward_Items reward_Items = (Reward_Items)list.FirstOrDefault((Reward x) => x is Reward_Items);
			if (reward_Items != null)
			{
				bool flag = false;
				for (int num2 = 0; num2 < generatedRewards.Count; num2++)
				{
					Reward_Items otherGeneratedItems = null;
					for (int num3 = 0; num3 < generatedRewards[num2].Count; num3++)
					{
						otherGeneratedItems = generatedRewards[num2][num3] as Reward_Items;
						if (otherGeneratedItems != null)
						{
							break;
						}
					}
					if (otherGeneratedItems != null)
					{
						int k;
						for (k = 0; k < otherGeneratedItems.items.Count; k++)
						{
							if (reward_Items.items.Any((Thing x) => x.GetInnerIfMinified().def == otherGeneratedItems.items[k].GetInnerIfMinified().def))
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

	private static int GetDisplayPriority(QuestPart_Choice.Choice choice)
	{
		for (int i = 0; i < choice.rewards.Count; i++)
		{
			if (choice.rewards[i] is Reward_ReimplantXenogerm)
			{
				return 2;
			}
		}
		for (int j = 0; j < choice.rewards.Count; j++)
		{
			if (choice.rewards[j] is Reward_RoyalFavor)
			{
				return 1;
			}
		}
		for (int k = 0; k < choice.rewards.Count; k++)
		{
			if (choice.rewards[k] is Reward_Goodwill)
			{
				return -1;
			}
		}
		return 0;
	}
}
