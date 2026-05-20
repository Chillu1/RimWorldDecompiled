using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_Carnivore : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!FoodUtility.AcceptableVegetarian(t) && FoodUtility.AcceptableCarnivore(t) && !FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(t))
			{
				return !FoodUtility.IsInsectCorpseOrInsectMeatIngredient(t);
			}
			return false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (FoodUtility.UnacceptableVegetarian(def) && FoodUtility.GetMeatSourceCategory(def) != MeatSourceCategory.Humanlike)
			{
				return FoodUtility.GetMeatSourceCategory(def) != MeatSourceCategory.Insect;
			}
			return false;
		}
	}
}
