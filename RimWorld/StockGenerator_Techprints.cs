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
			for (int i = 0; i < countToGenerate; i++)
			{
				if (!TechprintUtility.TryGetTechprintDefToGenerate(faction, out ThingDef result, tmpGenerated))
				{
					yield break;
				}
				tmpGenerated.Add(result);
				foreach (Thing item in StockGeneratorUtility.TryMakeForStock(result, 1))
				{
					yield return item;
				}
			}
			tmpGenerated.Clear();
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
