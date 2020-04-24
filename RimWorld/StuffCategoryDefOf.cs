namespace RimWorld
{
	[DefOf]
	public static class StuffCategoryDefOf
	{
		public static StuffCategoryDef Metallic;

		public static StuffCategoryDef Woody;

		public static StuffCategoryDef Stony;

		public static StuffCategoryDef Fabric;

		public static StuffCategoryDef Leathery;

		static StuffCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(StuffCategoryDefOf));
		}
	}
}
