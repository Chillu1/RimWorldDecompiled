using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Reward_ShuttleLoot : Reward
{
	private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Icons/CrashedShuttle");

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			yield return QuestPartUtility.GetStandardRewardStackElement("Reward_ShuttleLoot_Label".Translate(), Icon, () => GetDescription(default(RewardsGeneratorParams)).CapitalizeFirst() + ".");
		}
	}

	public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		throw new NotImplementedException();
	}

	public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
	{
		throw new NotImplementedException();
	}

	public override string GetDescription(RewardsGeneratorParams parms)
	{
		return "Reward_ShuttleLoot".Translate().Resolve();
	}
}
