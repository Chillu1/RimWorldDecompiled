namespace RimWorld
{
	[DefOf]
	public static class RoyalTitleDefOf
	{
		[MayRequireRoyalty]
		public static RoyalTitleDef Knight;

		[MayRequireRoyalty]
		public static RoyalTitleDef Count;

		static RoyalTitleDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RoyalTitleDefOf));
		}
	}
}
