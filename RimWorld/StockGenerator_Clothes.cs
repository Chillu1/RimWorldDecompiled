using Verse;

namespace RimWorld
{
	public class StockGenerator_Clothes : StockGenerator_MiscItems
	{
		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(500f, 0.5f),
			new CurvePoint(1500f, 0.2f),
			new CurvePoint(5000f, 0.1f)
		};

		public string apparelTag;

		public override bool HandlesThingDef(ThingDef td)
		{
			if (td == ThingDefOf.Apparel_ShieldBelt)
			{
				return false;
			}
			if (base.HandlesThingDef(td) && td.IsApparel && (apparelTag == null || (td.apparel.tags != null && td.apparel.tags.Contains(apparelTag))))
			{
				if (!(td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt) < 0.15f))
				{
					return td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp) < 0.15f;
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
