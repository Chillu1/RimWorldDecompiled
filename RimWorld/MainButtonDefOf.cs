namespace RimWorld
{
	[DefOf]
	public static class MainButtonDefOf
	{
		public static MainButtonDef Inspect;

		public static MainButtonDef Architect;

		public static MainButtonDef Research;

		public static MainButtonDef Menu;

		public static MainButtonDef World;

		public static MainButtonDef Quests;

		public static MainButtonDef Factions;

		static MainButtonDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MainButtonDefOf));
		}
	}
}
