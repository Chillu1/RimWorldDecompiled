using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class StockGenerator_MiscItems : StockGenerator
	{
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			int count = countRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				if (!DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => HandlesThingDef(t) && t.tradeability.TraderCanSell() && (int)t.techLevel <= (int)maxTechLevelGenerate).TryRandomElementByWeight(SelectionWeight, out var result))
				{
					break;
				}
				yield return MakeThing(result);
			}
		}

		protected virtual Thing MakeThing(ThingDef def)
		{
			return StockGeneratorUtility.TryMakeForStockSingle(def, 1);
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.tradeability != 0)
			{
				return (int)thingDef.techLevel <= (int)maxTechLevelBuy;
			}
			return false;
		}

		protected virtual float SelectionWeight(ThingDef thingDef)
		{
			return 1f;
		}
	}
}
