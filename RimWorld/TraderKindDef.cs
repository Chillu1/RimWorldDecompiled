using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class TraderKindDef : Def
{
	public List<StockGenerator> stockGenerators = new List<StockGenerator>();

	public bool orbital;

	public bool requestable = true;

	public bool hideThingsNotWillingToTrade;

	public float commonality = 1f;

	public string category;

	public TradeCurrency tradeCurrency;

	public SimpleCurve commonalityMultFromPopulationIntent;

	public FactionDef faction;

	public RoyalTitlePermitDef permitRequiredForTrading;

	public float CalculatedCommonality
	{
		get
		{
			float num = commonality;
			if (commonalityMultFromPopulationIntent != null)
			{
				num *= commonalityMultFromPopulationIntent.Evaluate(StorytellerUtilityPopulation.PopulationIntent);
			}
			return num;
		}
	}

	public RoyalTitleDef TitleRequiredToTrade
	{
		get
		{
			if (permitRequiredForTrading != null)
			{
				RoyalTitleDef royalTitleDef = faction.RoyalTitlesAwardableInSeniorityOrderForReading.FirstOrDefault((RoyalTitleDef x) => x.permits != null && x.permits.Contains(permitRequiredForTrading));
				if (royalTitleDef != null)
				{
					return royalTitleDef;
				}
			}
			return null;
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();
		foreach (StockGenerator stockGenerator in stockGenerators)
		{
			stockGenerator.ResolveReferences(this);
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		foreach (StockGenerator stockGenerator in stockGenerators)
		{
			foreach (string item2 in stockGenerator.ConfigErrors(this))
			{
				yield return item2;
			}
		}
	}

	public bool WillTrade(ThingDef td)
	{
		for (int i = 0; i < stockGenerators.Count; i++)
		{
			if (stockGenerators[i].HandlesThingDef(td))
			{
				return true;
			}
		}
		return false;
	}

	public PriceType PriceTypeFor(ThingDef thingDef, TradeAction action)
	{
		if (thingDef == ThingDefOf.Silver)
		{
			return PriceType.Undefined;
		}
		if (action == TradeAction.PlayerBuys)
		{
			for (int i = 0; i < stockGenerators.Count; i++)
			{
				if (stockGenerators[i].TryGetPriceType(thingDef, action, out var priceType))
				{
					return priceType;
				}
			}
		}
		return PriceType.Normal;
	}
}
