using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_Cannibal : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			return FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(t);
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (!def.IsCorpse && !def.HasComp(typeof(CompIngredients)))
			{
				return FoodUtility.GetMeatSourceCategory(def) == MeatSourceCategory.Humanlike;
			}
			return true;
		}
	}
}
