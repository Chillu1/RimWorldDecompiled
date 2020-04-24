namespace RimWorld
{
	[DefOf]
	public static class WorkGiverDefOf
	{
		public static WorkGiverDef Refuel;

		public static WorkGiverDef Repair;

		static WorkGiverDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(WorkGiverDefOf));
		}
	}
}
