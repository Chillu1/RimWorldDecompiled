using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StockGenerator_BuySlaves : StockGenerator
	{
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			yield break;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike)
			{
				return thingDef.tradeability != Tradeability.None;
			}
			return false;
		}
	}
}
