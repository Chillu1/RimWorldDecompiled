namespace RimWorld
{
	[DefOf]
	public static class GatheringDefOf
	{
		public static GatheringDef Party;

		public static GatheringDef MarriageCeremony;

		static GatheringDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GatheringDefOf));
		}
	}
}
