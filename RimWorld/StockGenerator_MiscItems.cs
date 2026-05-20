using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public abstract class StockGenerator_MiscItems : StockGenerator
{
	public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
	{
		int count = countRange.RandomInRange;
		for (int i = 0; i < count; i++)
		{
			if (!DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => HandlesThingDef(t) && t.tradeability.TraderCanSell() && (int)t.techLevel <= (int)maxTechLevelGenerate).TryRandomElementByWeight(SelectionWeight, out var result))
			{
				break;
			}
			yield return MakeThing(result, faction);
		}
	}

	protected virtual Thing MakeThing(ThingDef def, Faction faction)
	{
		return StockGeneratorUtility.TryMakeForStockSingle(def, 1, faction);
	}

	public override bool HandlesThingDef(ThingDef thingDef)
	{
		if (thingDef.tradeability != Tradeability.None)
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
