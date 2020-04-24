namespace RimWorld
{
	[DefOf]
	public static class PawnGroupKindDefOf
	{
		public static PawnGroupKindDef Combat;

		public static PawnGroupKindDef Trader;

		public static PawnGroupKindDef Peaceful;

		public static PawnGroupKindDef Settlement;

		static PawnGroupKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnGroupKindDefOf));
		}
	}
}
