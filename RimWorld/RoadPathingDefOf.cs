namespace RimWorld
{
	[DefOf]
	public static class RoadPathingDefOf
	{
		public static RoadPathingDef Avoid;

		public static RoadPathingDef Bulldoze;

		static RoadPathingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RoadPathingDefOf));
		}
	}
}
