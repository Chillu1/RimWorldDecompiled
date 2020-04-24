using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class RecipeWorkerCounter_MakeStoneBlocks : RecipeWorkerCounter
	{
		public override bool CanCountProducts(Bill_Production bill)
		{
			return true;
		}

		public override int CountProducts(Bill_Production bill)
		{
			int num = 0;
			List<ThingDef> childThingDefs = ThingCategoryDefOf.StoneBlocks.childThingDefs;
			for (int i = 0; i < childThingDefs.Count; i++)
			{
				num += bill.Map.resourceCounter.GetCount(childThingDefs[i]);
			}
			return num;
		}

		public override string ProductsDescription(Bill_Production bill)
		{
			return ThingCategoryDefOf.StoneBlocks.label;
		}

		public override bool CanPossiblyStoreInStockpile(Bill_Production bill, Zone_Stockpile stockpile)
		{
			foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
			{
				if (!allowedThingDef.butcherProducts.NullOrEmpty())
				{
					ThingDef thingDef = allowedThingDef.butcherProducts[0].thingDef;
					if (!stockpile.GetStoreSettings().AllowedToAccept(thingDef))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
