using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_NonBurnableWeapons : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!CanEverMatch(t.def))
			{
				return false;
			}
			return !t.BurnableByRecipe;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (!def.IsWeapon)
			{
				return false;
			}
			if (!def.thingCategories.NullOrEmpty())
			{
				for (int i = 0; i < def.thingCategories.Count; i++)
				{
					for (ThingCategoryDef thingCategoryDef = def.thingCategories[i]; thingCategoryDef != null; thingCategoryDef = thingCategoryDef.parent)
					{
						if (thingCategoryDef == ThingCategoryDefOf.Weapons)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			if (CanEverMatch(def) && !def.burnableByRecipe)
			{
				return !def.MadeFromStuff;
			}
			return false;
		}
	}
}
