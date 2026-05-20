using System.Collections.Generic;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Reward_RoyalFavor : Reward
{
	public int amount;

	public Faction faction;

	public override bool MakesUseOfChosenPawnSignal => true;

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			yield return QuestPartUtility.GetStandardRewardStackElement(faction.def.royalFavorLabel.CapitalizeFirst() + " " + amount.ToStringWithSign(), faction.def.RoyalFavorIcon, () => ("RoyalFavorTip".Translate(Faction.OfPlayer.def.pawnsPlural, amount, faction.def.royalFavorLabel, faction) + "\n\n" + "ClickForMoreInfo".Translate().Colorize(ColoredText.SubtleGrayColor)).Resolve(), delegate
			{
				Find.WindowStack.Add(new Dialog_InfoCard(faction));
			});
		}
	}

	public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		amount = GenMath.RoundRandom(RewardsGenerator.RewardValueToRoyalFavorCurve.Evaluate(rewardValue));
		amount = Mathf.Clamp(amount, 1, 12);
		valueActuallyUsed = RewardsGenerator.RewardValueToRoyalFavorCurve.EvaluateInverted(amount);
		faction = parms.giverFaction;
	}

	public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
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
		yield return questPart_GiveRoyalFavor;
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

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref amount, "amount", 0);
		Scribe_References.Look(ref faction, "faction");
	}
}
