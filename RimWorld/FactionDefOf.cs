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

		[MayRequireRoyalty]
		public static FactionDef Empire;

		static FactionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
		}
	}
}
