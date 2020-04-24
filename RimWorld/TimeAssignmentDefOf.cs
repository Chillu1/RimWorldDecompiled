namespace RimWorld
{
	[DefOf]
	public static class TimeAssignmentDefOf
	{
		public static TimeAssignmentDef Anything;

		public static TimeAssignmentDef Work;

		public static TimeAssignmentDef Joy;

		public static TimeAssignmentDef Sleep;

		static TimeAssignmentDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TimeAssignmentDefOf));
		}
	}
}
