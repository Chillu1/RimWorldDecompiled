using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Reward_Items : Reward
{
	public struct RememberedItem : IExposable
	{
		public ThingStuffPairWithQuality thing;

		public int stackCount;

		public string label;

		public RememberedItem(ThingStuffPairWithQuality thing, int stackCount, string label)
		{
			this.thing = thing;
			this.stackCount = stackCount;
			this.label = label;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref thing, "thing");
			Scribe_Values.Look(ref stackCount, "stackCount", 0);
			Scribe_Values.Look(ref label, "label");
		}
	}

	public List<Thing> items = new List<Thing>();

	private List<RememberedItem> itemDefs = new List<RememberedItem>();

	private float lastTotalMarketValue;

	private const string RootSymbol = "root";

	public List<Thing> ItemsListForReading => items;

	public override IEnumerable<GenUI.AnonymousStackElement> StackElements
	{
		get
		{
			if (usedOrCleanedUp)
			{
				foreach (GenUI.AnonymousStackElement rewardStackElementsForThing in QuestPartUtility.GetRewardStackElementsForThings(itemDefs))
				{
					yield return rewardStackElementsForThing;
				}
				yield break;
			}
			foreach (GenUI.AnonymousStackElement rewardStackElementsForThing2 in QuestPartUtility.GetRewardStackElementsForThings(items))
			{
				yield return rewardStackElementsForThing2;
			}
		}
	}

	public override float TotalMarketValue
	{
		get
		{
			if (usedOrCleanedUp)
			{
				return lastTotalMarketValue;
			}
			float num = 0f;
			for (int i = 0; i < items.Count; i++)
			{
				Thing innerIfMinified = items[i].GetInnerIfMinified();
				num += innerIfMinified.MarketValue * (float)items[i].stackCount;
			}
			return num;
		}
	}

	public override void Notify_Used()
	{
		RememberItems();
		base.Notify_Used();
	}

	public override void Notify_PreCleanup()
	{
		RememberItems();
		base.Notify_PreCleanup();
	}

	private void RememberItems()
	{
		if (usedOrCleanedUp)
		{
			return;
		}
		itemDefs.Clear();
		lastTotalMarketValue = 0f;
		for (int i = 0; i < items.Count; i++)
		{
			Thing innerIfMinified = items[i].GetInnerIfMinified();
			if (innerIfMinified != null && !innerIfMinified.Destroyed)
			{
				if (!innerIfMinified.TryGetQuality(out var qc))
				{
					qc = QualityCategory.Normal;
				}
				itemDefs.Add(new RememberedItem(new ThingStuffPairWithQuality(innerIfMinified.def, innerIfMinified.Stuff, qc), items[i].stackCount, items[i].LabelNoCount));
				lastTotalMarketValue += innerIfMinified.MarketValue * (float)items[i].stackCount;
			}
		}
	}

	public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
	{
		items.Clear();
		bool flag = true;
		float x = (Find.TickManager.TicksGame - Find.History.lastPsylinkAvailable).TicksToDays();
		if (Rand.Chance(QuestTuning.DaysSincePsylinkAvailableToGuaranteedNeuroformerChance.Evaluate(x)) && ModsConfig.RoyaltyActive && (parms.disallowedThingDefs == null || !parms.disallowedThingDefs.Contains(ThingDefOf.PsychicAmplifier)) && rewardValue >= 600f && (Faction.OfEmpire == null || parms.giverFaction != Faction.OfEmpire))
		{
			items.Add(ThingMaker.MakeThing(ThingDefOf.PsychicAmplifier));
			rewardValue -= items[0].MarketValue;
			if (rewardValue < 100f)
			{
				flag = false;
			}
		}
		if (flag)
		{
			FloatRange value = rewardValue * new FloatRange(0.7f, 1.3f);
			ThingSetMakerParams parms2 = new ThingSetMakerParams
			{
				totalMarketValueRange = value,
				makingFaction = parms.giverFaction
			};
			if (!parms.disallowedThingDefs.NullOrEmpty())
			{
				parms2.validator = (ThingDef item) => !parms.disallowedThingDefs.Contains(item);
			}
			items.AddRange(ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms2));
		}
		valueActuallyUsed = TotalMarketValue;
	}

	public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
	{
		Slate slate = RimWorld.QuestGen.QuestGen.slate;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] is Pawn pawn)
			{
				RimWorld.QuestGen.QuestGen.AddToGeneratedPawns(pawn);
				if (!pawn.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
			}
		}
		if (parms.giveToCaravan)
		{
			QuestPart_GiveToCaravan questPart_GiveToCaravan = new QuestPart_GiveToCaravan();
			questPart_GiveToCaravan.inSignal = slate.Get<string>("inSignal");
			questPart_GiveToCaravan.Things = items;
			yield return questPart_GiveToCaravan;
		}
		else
		{
			QuestPart_DropPods dropPods = new QuestPart_DropPods();
			dropPods.inSignal = slate.Get<string>("inSignal");
			if (!customLetterLabel.NullOrEmpty() || customLetterLabelRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddTextRequest("root", delegate(string x)
				{
					dropPods.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules, customLetterLabel, "root"));
			}
			if (!customLetterText.NullOrEmpty() || customLetterTextRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddTextRequest("root", delegate(string x)
				{
					dropPods.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules, customLetterText, "root"));
			}
			dropPods.mapParent = slate.Get<Map>("map").Parent;
			dropPods.useTradeDropSpot = true;
			dropPods.Things = items;
			yield return dropPods;
		}
		slate.Set("itemsReward_items", items);
		slate.Set("itemsReward_totalMarketValue", TotalMarketValue);
	}

	public override string GetDescription(RewardsGeneratorParams parms)
	{
		if (parms.giveToCaravan)
		{
			return "Reward_Items_Caravan".Translate(GenLabel.ThingsLabel(items), TotalMarketValue.ToStringMoney());
		}
		return "Reward_Items".Translate(GenLabel.ThingsLabel(items), TotalMarketValue.ToStringMoney());
	}

	public override string ToString()
	{
		string name = GetType().Name;
		name = name + "(value " + TotalMarketValue.ToStringMoney() + ")";
		foreach (Thing item in items)
		{
			name = name + "\n  -" + item.LabelCap + " " + (item.MarketValue * (float)item.stackCount).ToStringMoney();
		}
		return name;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref items, "items", LookMode.Reference);
		Scribe_Collections.Look(ref itemDefs, "itemDefs", LookMode.Deep);
		Scribe_Values.Look(ref lastTotalMarketValue, "lastTotalMarketValue", 0f);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			items.RemoveAll((Thing x) => x == null);
		}
	}
}
