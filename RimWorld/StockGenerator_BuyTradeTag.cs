using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StockGenerator_BuyTradeTag : StockGenerator
{
	public string tag;

	public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
	{
		return Enumerable.Empty<Thing>();
	}

	public override bool HandlesThingDef(ThingDef thingDef)
	{
		if (thingDef.tradeTags != null)
		{
			return thingDef.tradeTags.Contains(tag);
		}
		return false;
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
