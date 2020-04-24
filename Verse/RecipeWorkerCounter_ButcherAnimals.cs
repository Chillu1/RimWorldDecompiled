using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class RecipeWorkerCounter_ButcherAnimals : RecipeWorkerCounter
	{
		public override bool CanCountProducts(Bill_Production bill)
		{
			return true;
		}

		public override int CountProducts(Bill_Production bill)
		{
			int num = 0;
			List<ThingDef> childThingDefs = ThingCategoryDefOf.MeatRaw.childThingDefs;
			for (int i = 0; i < childThingDefs.Count; i++)
			{
				num += bill.Map.resourceCounter.GetCount(childThingDefs[i]);
			}
			return num;
		}

		public override string ProductsDescription(Bill_Production bill)
		{
			return ThingCategoryDefOf.MeatRaw.label;
		}

		public override bool CanPossiblyStoreInStockpile(Bill_Production bill, Zone_Stockpile stockpile)
		{
			foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
			{
				if (allowedThingDef.ingestible != null && allowedThingDef.ingestible.sourceDef != null)
				{
					RaceProperties race = allowedThingDef.ingestible.sourceDef.race;
					if (race != null && race.meatDef != null && !stockpile.GetStoreSettings().AllowedToAccept(race.meatDef))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
