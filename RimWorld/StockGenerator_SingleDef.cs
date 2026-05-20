using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StockGenerator_SingleDef : StockGenerator
{
	private ThingDef thingDef;

	public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
	{
		foreach (Thing item in StockGeneratorUtility.TryMakeForStock(thingDef, RandomCountOf(thingDef), faction))
		{
			yield return item;
		}
	}

	public override bool HandlesThingDef(ThingDef thingDef)
	{
		return thingDef == this.thingDef;
	}

	public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (!thingDef.tradeability.TraderCanSell())
		{
			yield return thingDef?.ToString() + " tradeability doesn't allow traders to sell this thing";
		}
	}
}
