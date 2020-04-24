using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuySingleDef : StockGenerator
	{
		public ThingDef thingDef;

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			yield break;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef == this.thingDef;
		}
	}
}
