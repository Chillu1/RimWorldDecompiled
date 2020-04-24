using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnerItems : CompProperties
	{
		public float approxMarketValuePerDay;

		public int spawnInterval = 60000;

		public List<StuffCategoryDef> stuffCategories = new List<StuffCategoryDef>();

		public List<ThingCategoryDef> categories = new List<ThingCategoryDef>();

		public IEnumerable<ThingDef> MatchingItems => DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) => def.BaseMarketValue <= approxMarketValuePerDay && ((def.IsStuff && stuffCategories.Any((StuffCategoryDef c) => def.stuffProps.categories.Contains(c))) || categories.Any((ThingCategoryDef c) => def.IsWithinCategory(c))));

		public CompProperties_SpawnerItems()
		{
			compClass = typeof(CompSpawnerItems);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (MatchingItems.Count() <= 0)
			{
				yield return "Could not find any item that would be spawnable by " + parentDef.defName + " (CompSpawnerItems)!";
			}
		}
	}
}
