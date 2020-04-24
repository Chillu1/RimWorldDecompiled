using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuyTradeTag : StockGenerator
	{
		public string tag;

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			yield break;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.tradeTags != null)
			{
				return thingDef.tradeTags.Contains(tag);
			}
			return false;
		}
	}
}
