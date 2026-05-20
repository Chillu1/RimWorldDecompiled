using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PreceptWorker_Animal : PreceptWorker
{
	public override IEnumerable<PreceptThingChance> ThingDefs
	{
		get
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.thingCategories != null && x.thingCategories.Contains(ThingCategoryDefOf.Animals) && !x.race.Insect && !x.race.Dryad))
			{
				yield return new PreceptThingChance
				{
					def = item,
					chance = 1f
				};
			}
		}
	}

	public override AcceptanceReport CanUse(ThingDef def, Ideo ideo, FactionDef generatingFor)
	{
		foreach (Precept item in ideo.PreceptsListForReading)
		{
			if (item is Precept_Animal precept_Animal && precept_Animal.ThingDef == def)
			{
				return false;
			}
		}
		return true;
	}
}
