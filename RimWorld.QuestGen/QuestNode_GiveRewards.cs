using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

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
		QuestGen.quest.GiveRewards(parms.GetValue(QuestGen.slate), inSignal.GetValue(QuestGen.slate), customLetterLabel.GetValue(QuestGen.slate), customLetterText.GetValue(QuestGen.slate), customLetterLabelRules.GetValue(QuestGen.slate), customLetterTextRules.GetValue(QuestGen.slate), useDifficultyFactor.GetValue(QuestGen.slate), delegate
		{
			nodeIfChosenPawnSignalUsed?.Run();
		}, variants.GetValue(QuestGen.slate), addCampLootReward.GetValue(QuestGen.slate), QuestGen.slate.Get<Pawn>("asker"));
	}

	[Obsolete("Will be removed in the future")]
	private List<Reward> GenerateRewards(RewardsGeneratorParams parmsResolved, Slate slate, bool addDescriptionRules, ref bool chosenPawnSignalUsed, QuestPart_Choice.Choice choice, int variants)
	{
		return null;
	}

	[Obsolete("Will be removed in the future")]
	private List<Reward> TryGenerateRewards_SocialOnly(RewardsGeneratorParams parms, bool disallowRoyalFavor)
	{
		return null;
	}

	[Obsolete("Will be removed in the future")]
	private List<Reward> TryGenerateRewards_RoyalFavorOnly(RewardsGeneratorParams parms)
	{
		return null;
	}

	[Obsolete("Will be removed in the future")]
	private List<Reward> TryGenerateRewards_ThingsOnly(RewardsGeneratorParams parms)
	{
		return null;
	}

	[Obsolete("Will be removed in the future")]
	private List<Reward> TryGenerateNonRepeatingRewards(RewardsGeneratorParams parms)
	{
		return null;
	}

	[Obsolete("Will be removed in the future")]
	private int GetDisplayPriority(QuestPart_Choice.Choice choice)
	{
		return 0;
	}
}
