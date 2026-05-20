using System.Collections.Generic;
using RimWorld;

namespace Verse;

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

	public override bool CanPossiblyStore(Bill_Production bill, ISlotGroup slotGroup)
	{
		foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
		{
			if (allowedThingDef.ingestible != null && allowedThingDef.ingestible.sourceDef != null)
			{
				RaceProperties race = allowedThingDef.ingestible.sourceDef.race;
				if (race != null && race.meatDef != null && !slotGroup.Settings.AllowedToAccept(race.meatDef))
				{
					return false;
				}
			}
		}
		return true;
	}
}
