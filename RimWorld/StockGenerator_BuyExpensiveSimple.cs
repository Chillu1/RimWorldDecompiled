using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StockGenerator_BuyExpensiveSimple : StockGenerator
{
	public float minValuePerUnit = 15f;

	public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
	{
		return Enumerable.Empty<Thing>();
	}

	public override bool HandlesThingDef(ThingDef thingDef)
	{
		if (thingDef.category != ThingCategory.Item || thingDef.IsApparel || thingDef.IsWeapon || thingDef.IsMedicine || thingDef.IsDrug || !thingDef.genericMarketSellable)
		{
			return false;
		}
		if (thingDef == ThingDefOf.InsectJelly)
		{
			return true;
		}
		return thingDef.BaseMarketValue / thingDef.VolumePerUnit >= minValuePerUnit;
	}

	public override Tradeability TradeabilityFor(ThingDef thingDef)
	{
		if (thingDef.tradeability == Tradeability.None || !HandlesThingDef(thingDef))
		{
			return Tradeability.None;
		}
		return Tradeability.Sellable;
	}
}
