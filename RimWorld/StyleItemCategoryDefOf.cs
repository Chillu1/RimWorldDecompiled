namespace RimWorld
{
	[DefOf]
	public static class StyleItemCategoryDefOf
	{
		public static StyleItemCategoryDef Misc;

		static StyleItemCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(StyleItemCategoryDefOf));
		}
	}
}
