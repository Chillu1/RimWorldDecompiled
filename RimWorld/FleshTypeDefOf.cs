namespace RimWorld
{
	[DefOf]
	public static class FleshTypeDefOf
	{
		public static FleshTypeDef Normal;

		public static FleshTypeDef Mechanoid;

		public static FleshTypeDef Insectoid;

		static FleshTypeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(FleshTypeDefOf));
		}
	}
}
