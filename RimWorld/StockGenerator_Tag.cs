using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Tag : StockGenerator
	{
		[NoTranslate]
		private string tradeTag;

		private IntRange thingDefCountRange = IntRange.one;

		private List<ThingDef> excludedThingDefs;

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			StockGenerator_Tag stockGenerator_Tag = this;
			List<ThingDef> generatedDefs = new List<ThingDef>();
			int numThingDefsToUse = thingDefCountRange.RandomInRange;
			for (int i = 0; i < numThingDefsToUse; i++)
			{
				if (!DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => stockGenerator_Tag.HandlesThingDef(d) && d.tradeability.TraderCanSell() && (stockGenerator_Tag.excludedThingDefs == null || !stockGenerator_Tag.excludedThingDefs.Contains(d)) && !generatedDefs.Contains(d)).TryRandomElement(out ThingDef chosenThingDef))
				{
					break;
				}
				foreach (Thing item in StockGeneratorUtility.TryMakeForStock(chosenThingDef, RandomCountOf(chosenThingDef)))
				{
					yield return item;
				}
				generatedDefs.Add(chosenThingDef);
				chosenThingDef = null;
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.tradeTags != null && thingDef.tradeability != 0 && (int)thingDef.techLevel <= (int)maxTechLevelBuy)
			{
				return thingDef.tradeTags.Contains(tradeTag);
			}
			return false;
		}
	}
}
