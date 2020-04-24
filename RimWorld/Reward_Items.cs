using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Reward_Items : Reward
	{
		public List<Thing> items = new List<Thing>();

		private const string RootSymbol = "root";

		private float TotalMarketValue
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < items.Count; i++)
				{
					num += items[i].MarketValue * (float)items[i].stackCount;
				}
				return num;
			}
		}

		public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
		{
			FloatRange value = rewardValue * new FloatRange(0.7f, 1.3f);
			ThingSetMakerParams parms2 = default(ThingSetMakerParams);
			parms2.totalMarketValueRange = value;
			parms2.makingFaction = parms.giverFaction;
			if (!parms.disallowedThingDefs.NullOrEmpty())
			{
				parms2.validator = ((ThingDef x) => !parms.disallowedThingDefs.Contains(x));
			}
			items.Clear();
			items.AddRange(ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms2));
			valueActuallyUsed = TotalMarketValue;
		}

		public override void AddQuestPartsToGeneratingQuest(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
		{
			Slate slate = RimWorld.QuestGen.QuestGen.slate;
			for (int i = 0; i < items.Count; i++)
			{
				Pawn pawn = items[i] as Pawn;
				if (pawn != null)
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
				RimWorld.QuestGen.QuestGen.quest.AddPart(questPart_GiveToCaravan);
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
				RimWorld.QuestGen.QuestGen.quest.AddPart(dropPods);
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
	}
}
