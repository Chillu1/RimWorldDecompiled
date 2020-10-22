using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Techprints : StockGenerator
	{
		private List<CountChance> countChances;

		private List<ThingDef> tmpGenerated = new List<ThingDef>();

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			tmpGenerated.Clear();
			int countToGenerate = CountChanceUtility.RandomCount(countChances);
			int i = 0;
			while (true)
			{
				if (i < countToGenerate)
				{
					if (!TechprintUtility.TryGetTechprintDefToGenerate(faction, out var result, tmpGenerated))
					{
						break;
					}
					tmpGenerated.Add(result);
					foreach (Thing item in StockGeneratorUtility.TryMakeForStock(result, 1))
					{
						yield return item;
					}
					i++;
					continue;
				}
				tmpGenerated.Clear();
				break;
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.tradeTags != null && thingDef.tradeability != 0 && (int)thingDef.techLevel <= (int)maxTechLevelBuy)
			{
				return thingDef.tradeTags.Contains("Techprint");
			}
			return false;
		}
	}
}
