using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class StockGenerator
	{
		[Unsaved(false)]
		public TraderKindDef trader;

		public IntRange countRange = IntRange.zero;

		public List<ThingDefCountRangeClass> customCountRanges;

		public FloatRange totalPriceRange = FloatRange.Zero;

		public TechLevel maxTechLevelGenerate = TechLevel.Archotech;

		public TechLevel maxTechLevelBuy = TechLevel.Archotech;

		public PriceType price = PriceType.Normal;

		public virtual void ResolveReferences(TraderKindDef trader)
		{
			this.trader = trader;
		}

		public virtual IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
		{
			yield break;
		}

		public abstract IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null);

		public abstract bool HandlesThingDef(ThingDef thingDef);

		public bool TryGetPriceType(ThingDef thingDef, TradeAction action, out PriceType priceType)
		{
			if (!HandlesThingDef(thingDef))
			{
				priceType = PriceType.Undefined;
				return false;
			}
			priceType = price;
			return true;
		}

		protected int RandomCountOf(ThingDef def)
		{
			IntRange intRange = countRange;
			if (customCountRanges != null)
			{
				for (int i = 0; i < customCountRanges.Count; i++)
				{
					if (customCountRanges[i].thingDef == def)
					{
						intRange = customCountRanges[i].countRange;
						break;
					}
				}
			}
			if (intRange.max <= 0 && totalPriceRange.max <= 0f)
			{
				return 0;
			}
			if (intRange.max > 0 && totalPriceRange.max <= 0f)
			{
				return intRange.RandomInRange;
			}
			if (intRange.max <= 0 && totalPriceRange.max > 0f)
			{
				return Mathf.RoundToInt(totalPriceRange.RandomInRange / def.BaseMarketValue);
			}
			int num = 0;
			int randomInRange;
			do
			{
				randomInRange = intRange.RandomInRange;
				num++;
			}
			while (num <= 100 && !totalPriceRange.Includes((float)randomInRange * def.BaseMarketValue));
			return randomInRange;
		}
	}
}
