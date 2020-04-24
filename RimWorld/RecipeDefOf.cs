using Verse;

namespace RimWorld
{
	[DefOf]
	public static class RecipeDefOf
	{
		public static RecipeDef RemoveBodyPart;

		public static RecipeDef CookMealSimple;

		public static RecipeDef InstallPegLeg;

		static RecipeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RecipeDefOf));
		}
	}
}
