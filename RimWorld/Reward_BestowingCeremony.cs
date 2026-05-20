using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Reward_BestowingCeremony : Reward
{
	private static readonly Texture2D IconPsylink = ContentFinder<Texture2D>.Get("Things/Item/Special/PsylinkNeuroformer");

	public string targetPawnName;

	public string titleName;

	public RoyalTitleDef royalTitle;

	public bool givePsylink;

	public Faction awardingFaction;

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			if (givePsylink)
			{
				yield return QuestPartUtility.GetStandardRewardStackElement("Reward_BestowingCeremony_Label".Translate(), IconPsylink, () => GetDescription(default(RewardsGeneratorParams)).CapitalizeFirst() + ".", delegate
				{
					Find.WindowStack.Add(new Dialog_InfoCard(HediffDefOf.PsychicAmplifier));
				});
			}
			yield return QuestPartUtility.GetStandardRewardStackElement("Reward_Title_Label".Translate(titleName.Named("TITLE")), awardingFaction.def.FactionIcon, () => "Reward_Title".Translate(targetPawnName.Named("PAWNNAME"), titleName.Named("TITLE"), awardingFaction.Named("FACTION")).Resolve() + ".", delegate
			{
				if (royalTitle != null)
				{
					Find.WindowStack.Add(new Dialog_InfoCard(royalTitle, awardingFaction));
				}
			});
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
		return "Reward_BestowingCeremony".Translate(targetPawnName.Named("PAWNNAME")).Resolve();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref targetPawnName, "targetPawnName");
		Scribe_Values.Look(ref titleName, "titleName");
		Scribe_Values.Look(ref givePsylink, "givePsylink", defaultValue: false);
		Scribe_References.Look(ref awardingFaction, "awardingFaction");
		Scribe_Defs.Look(ref royalTitle, "royalTitle");
	}
}
