namespace RimWorld
{
	[DefOf]
	public static class GatheringDefOf
	{
		public static GatheringDef Party;

		public static GatheringDef MarriageCeremony;

		[MayRequireRoyalty]
		public static GatheringDef ThroneSpeech;

		static GatheringDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GatheringDefOf));
		}
	}
}
