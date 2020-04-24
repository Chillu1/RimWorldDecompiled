using Verse;

namespace RimWorld
{
	public class StockGenerator_Art : StockGenerator_MiscItems
	{
		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(500f, 1f),
			new CurvePoint(1000f, 0.2f)
		};

		public override bool HandlesThingDef(ThingDef td)
		{
			if (base.HandlesThingDef(td) && td.Minifiable && td.category == ThingCategory.Building)
			{
				return td.thingClass == typeof(Building_Art);
			}
			return false;
		}

		protected override float SelectionWeight(ThingDef thingDef)
		{
			return SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}
	}
}
