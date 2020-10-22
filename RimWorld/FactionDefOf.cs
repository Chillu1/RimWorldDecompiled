namespace RimWorld
{
	[DefOf]
	public static class FactionDefOf
	{
		public static FactionDef PlayerColony;

		public static FactionDef PlayerTribe;

		public static FactionDef Ancients;

		public static FactionDef AncientsHostile;

		public static FactionDef Mechanoid;

		public static FactionDef Insect;

		public static FactionDef OutlanderCivil;

		[MayRequireRoyalty]
		public static FactionDef Empire;

		[MayRequireRoyalty]
		public static FactionDef OutlanderRefugee;

		static FactionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
		}
	}
}
