using System.Collections.Generic;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Reward_Goodwill : Reward
{
	public int amount;

	public Faction faction;

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			yield return QuestPartUtility.GetStandardRewardStackElement("Goodwill".Translate() + " " + amount.ToStringWithSign(), delegate(Rect r)
			{
				GUI.color = faction.Color;
				GUI.DrawTexture(r, faction.def.FactionIcon);
				GUI.color = Color.white;
			}, () => "GoodwillTip".Translate(faction, amount, -75, 75, faction.PlayerGoodwill, faction.PlayerRelationKind.GetLabelCap()).Resolve(), delegate
			{
				Find.WindowStack.Add(new Dialog_InfoCard(faction));
			});
		}
	}

	public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		amount = GenMath.RoundRandom(RewardsGenerator.RewardValueToGoodwillCurve.Evaluate(rewardValue));
		amount = Mathf.Min(amount, 100 - parms.giverFaction.PlayerGoodwill);
		amount = Mathf.Max(amount, 1);
		valueActuallyUsed = RewardsGenerator.RewardValueToGoodwillCurve.EvaluateInverted(amount);
		if (parms.giverFaction.HostileTo(Faction.OfPlayer))
		{
			amount += Mathf.Clamp(-parms.giverFaction.PlayerGoodwill / 2, 0, amount);
			amount = Mathf.Min(amount, 100 - parms.giverFaction.PlayerGoodwill);
			if (amount < 1)
			{
				Log.Warning("Tried to use " + amount + " goodwill in Reward_Goodwill. A different reward type should have been chosen in this case.");
				amount = 1;
			}
		}
		faction = parms.giverFaction;
	}

	public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
	{
		QuestPart_FactionGoodwillChange questPart_FactionGoodwillChange = new QuestPart_FactionGoodwillChange();
		questPart_FactionGoodwillChange.change = amount;
		questPart_FactionGoodwillChange.faction = faction;
		questPart_FactionGoodwillChange.inSignal = RimWorld.QuestGen.QuestGen.slate.Get<string>("inSignal");
		yield return questPart_FactionGoodwillChange;
	}

	public override string GetDescription(RewardsGeneratorParams parms)
	{
		return "Reward_Goodwill".Translate(faction, amount).Resolve();
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
