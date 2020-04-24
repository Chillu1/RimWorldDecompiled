namespace RimWorld
{
	[DefOf]
	public static class SitePartDefOf
	{
		public static SitePartDef Outpost;

		public static SitePartDef Turrets;

		public static SitePartDef Manhunters;

		public static SitePartDef SleepingMechanoids;

		public static SitePartDef AmbushHidden;

		public static SitePartDef AmbushEdge;

		public static SitePartDef PreciousLump;

		public static SitePartDef PossibleUnknownThreatMarker;

		static SitePartDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SitePartDefOf));
		}
	}
}
