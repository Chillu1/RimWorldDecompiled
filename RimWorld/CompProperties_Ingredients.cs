using Verse;

namespace RimWorld
{
	public class CompProperties_Ingredients : CompProperties
	{
		public bool performMergeCompatibilityChecks = true;

		public bool splitTransferableFoodKind;

		public FoodKind noIngredientsFoodKind = FoodKind.Any;

		public CompProperties_Ingredients()
		{
			compClass = typeof(CompIngredients);
		}
	}
}
