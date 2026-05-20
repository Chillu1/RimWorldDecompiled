using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class StockGenerator_Category : StockGenerator
{
	private ThingCategoryDef categoryDef;

	private IntRange thingDefCountRange = IntRange.One;

	private List<ThingDef> excludedThingDefs;

	private List<ThingCategoryDef> excludedCategories;

	public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
	{
		List<ThingDef> generatedDefs = new List<ThingDef>();
		int numThingDefsToUse = thingDefCountRange.RandomInRange;
		for (int i = 0; i < numThingDefsToUse; i++)
		{
			if (!categoryDef.DescendantThingDefs.Where((ThingDef t) => t.tradeability.TraderCanSell() && (int)t.techLevel <= (int)maxTechLevelGenerate && !generatedDefs.Contains(t) && (excludedThingDefs == null || !excludedThingDefs.Contains(t)) && (excludedCategories == null || !excludedCategories.Any((ThingCategoryDef c) => c.DescendantThingDefs.Contains(t)))).TryRandomElement(out var chosenThingDef))
			{
				break;
			}
			foreach (Thing item in StockGeneratorUtility.TryMakeForStock(chosenThingDef, RandomCountOf(chosenThingDef), faction))
			{
				yield return item;
			}
			generatedDefs.Add(chosenThingDef);
			chosenThingDef = null;
		}
	}

	public override bool HandlesThingDef(ThingDef t)
	{
		if (categoryDef.DescendantThingDefs.Contains(t) && t.tradeability != Tradeability.None && (int)t.techLevel <= (int)maxTechLevelBuy && (excludedThingDefs == null || !excludedThingDefs.Contains(t)))
		{
			if (excludedCategories != null)
			{
				return !excludedCategories.Any((ThingCategoryDef c) => c.DescendantThingDefs.Contains(t));
			}
			return true;
		}
		return false;
	}
}
