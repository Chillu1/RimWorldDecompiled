using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

[StaticConstructorOnStartup]
public class Reward_ReimplantXenogerm : Reward
{
	private static readonly CachedTexture Icon = new CachedTexture("UI/Icons/Genes/Gene_XenogermReimplanter");

	public const int ReimplantWaitDurationTicks = 15000;

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			yield return QuestPartUtility.GetStandardRewardStackElement("Reward_ReimplantXenogerm_Label".Translate(), Icon.Texture, () => GetDescription(default(RewardsGeneratorParams)).CapitalizeFirst());
		}
	}

	public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		throw new NotImplementedException();
	}

	public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
	{
		if (ModLister.CheckBiotech("reimplaning reward"))
		{
			Slate slate = QuestGen.slate;
			QuestPart_ReimplantXenogerm questPart_ReimplantXenogerm = new QuestPart_ReimplantXenogerm();
			questPart_ReimplantXenogerm.mapParent = slate.Get<Map>("map").Parent;
			questPart_ReimplantXenogerm.faction = slate.Get<Faction>("faction");
			questPart_ReimplantXenogerm.pawns.AddRange(slate.Get<List<Pawn>>("sanguophages"));
			questPart_ReimplantXenogerm.gatherSpot = slate.Get<IntVec3>("gatherSpot");
			questPart_ReimplantXenogerm.inSignal = QuestGen.slate.Get<string>("inSignal");
			questPart_ReimplantXenogerm.inSignalReimplanted = slate.Get<string>("xenogermReimplantedSignal");
			questPart_ReimplantXenogerm.waitDurationTicks = 15000;
			questPart_ReimplantXenogerm.inSignalRemovePawn = QuestGenUtility.HardcodedSignalWithQuestID("sanguophages.ChangedFactionToPlayer");
			yield return questPart_ReimplantXenogerm;
		}
	}

	public override string GetDescription(RewardsGeneratorParams parms)
	{
		return "Reward_ReimplantXenogerm_Desc".Translate().Resolve();
	}
}
