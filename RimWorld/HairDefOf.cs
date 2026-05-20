namespace RimWorld
{
	[DefOf]
	public static class HairDefOf
	{
		public static HairDef Bald;

		static HairDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HairDefOf));
		}
	}
}
