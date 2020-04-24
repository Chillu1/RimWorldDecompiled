namespace RimWorld
{
	[DefOf]
	public static class IncidentTargetTagDefOf
	{
		public static IncidentTargetTagDef World;

		public static IncidentTargetTagDef Caravan;

		public static IncidentTargetTagDef Map_RaidBeacon;

		public static IncidentTargetTagDef Map_PlayerHome;

		public static IncidentTargetTagDef Map_Misc;

		static IncidentTargetTagDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(IncidentTargetTagDefOf));
		}
	}
}
