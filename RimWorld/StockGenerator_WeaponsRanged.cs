using Verse;

namespace RimWorld
{
	public class StockGenerator_WeaponsRanged : StockGenerator_MiscItems
	{
		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(500f, 1f),
			new CurvePoint(1500f, 0.2f),
			new CurvePoint(5000f, 0.1f)
		};

		public string weaponTag;

		public override bool HandlesThingDef(ThingDef td)
		{
			if (base.HandlesThingDef(td) && td.IsRangedWeapon)
			{
				if (weaponTag != null)
				{
					if (td.weaponTags != null)
					{
						return td.weaponTags.Contains(weaponTag);
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
