namespace RimWorld
{
	[DefOf]
	public static class ResearchTabDefOf
	{
		public static ResearchTabDef Main;

		static ResearchTabDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ResearchTabDefOf));
		}
	}
}
