using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StockGenerator_BuySingleDef : StockGenerator
{
	public ThingDef thingDef;

	public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
	{
		return Enumerable.Empty<Thing>();
	}

	public override bool HandlesThingDef(ThingDef thingDef)
	{
		return thingDef == this.thingDef;
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
