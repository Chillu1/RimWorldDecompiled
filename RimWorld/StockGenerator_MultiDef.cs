using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StockGenerator_MultiDef : StockGenerator
	{
		private List<ThingDef> thingDefs = new List<ThingDef>();

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			ThingDef thingDef = thingDefs.RandomElement();
			foreach (Thing item in StockGeneratorUtility.TryMakeForStock(thingDef, RandomCountOf(thingDef)))
			{
				yield return item;
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDefs.Contains(thingDef);
		}

		public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
		{
			foreach (string item in base.ConfigErrors(parentDef))
			{
				yield return item;
			}
			for (int i = 0; i < thingDefs.Count; i++)
			{
				if (!thingDefs[i].tradeability.TraderCanSell())
				{
					yield return thingDefs[i] + " tradeability doesn't allow traders to sell this thing";
				}
			}
		}
	}
}
