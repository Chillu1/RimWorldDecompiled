namespace RimWorld
{
	[DefOf]
	public static class WorkGiverDefOf
	{
		public static WorkGiverDef Refuel;

		public static WorkGiverDef Repair;

		public static WorkGiverDef DoBillsMedicalHumanOperation;

		static WorkGiverDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(WorkGiverDefOf));
		}
	}
}
