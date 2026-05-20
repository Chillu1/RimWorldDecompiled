using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_InsectMeat : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			return FoodUtility.IsInsectCorpseOrInsectMeatIngredient(t);
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (!def.IsCorpse && !def.HasComp(typeof(CompIngredients)))
			{
				return FoodUtility.GetMeatSourceCategory(def) == MeatSourceCategory.Insect;
			}
			return true;
		}
	}
}
