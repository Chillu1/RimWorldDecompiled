namespace RimWorld
{
	[DefOf]
	public static class HibernatableStateDefOf
	{
		public static HibernatableStateDef Running;

		public static HibernatableStateDef Starting;

		public static HibernatableStateDef Hibernating;

		static HibernatableStateDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HibernatableStateDefOf));
		}
	}
}
