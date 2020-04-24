namespace RimWorld
{
	[DefOf]
	public static class AbilityDefOf
	{
		[MayRequireRoyalty]
		public static AbilityDef Speech;

		static AbilityDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
		}
	}
}
