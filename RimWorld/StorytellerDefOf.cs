namespace RimWorld
{
	[DefOf]
	public static class StorytellerDefOf
	{
		public static StorytellerDef Cassandra;

		public static StorytellerDef Tutor;

		static StorytellerDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(StorytellerDefOf));
		}
	}
}
