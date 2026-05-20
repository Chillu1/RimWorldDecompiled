using Verse;

namespace RimWorld
{
	public class StockGenerator_MarketValue : StockGenerator_MiscItems
	{
		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(500f, 0.5f),
			new CurvePoint(1500f, 0.2f),
			new CurvePoint(5000f, 0.1f)
		};

		[NoTranslate]
		public string tradeTag;

		[NoTranslate]
		public string weaponTag;

		[NoTranslate]
		public string apparelTag;

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (base.HandlesThingDef(thingDef) && thingDef.tradeTags.NotNullAndContains(tradeTag) && (weaponTag.NullOrEmpty() || thingDef.weaponTags.NotNullAndContains(weaponTag)))
			{
				if (!apparelTag.NullOrEmpty())
				{
					if (thingDef.apparel != null)
					{
						return thingDef.apparel.tags.NotNullAndContains(apparelTag);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		protected override float SelectionWeight(ThingDef thingDef)
		{
			return SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}
	}
}
