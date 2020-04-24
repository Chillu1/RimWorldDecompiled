namespace RimWorld
{
	[DefOf]
	public static class ScenarioDefOf
	{
		public static ScenarioDef Crashlanded;

		public static ScenarioDef Tutorial;

		static ScenarioDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ScenarioDefOf));
		}
	}
}
