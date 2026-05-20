using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Reward_DefinedThingDef : Reward
{
	public ThingDef thingDef;

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			yield return QuestPartUtility.GetStandardRewardStackElement(thingDef.LabelCap, Widgets.GetIconFor(thingDef), () => GetDescription(default(RewardsGeneratorParams)).CapitalizeFirst());
		}
	}

	public Reward_DefinedThingDef()
	{
	}

	public Reward_DefinedThingDef(ThingDef thingDef)
	{
		this.thingDef = thingDef;
	}

	public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		valueActuallyUsed = rewardValue;
	}

	public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
	{
		yield break;
	}

	public override string GetDescription(RewardsGeneratorParams parms)
	{
		return thingDef.description;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref thingDef, "thingDef");
	}
}
