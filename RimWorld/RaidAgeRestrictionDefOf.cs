namespace RimWorld
{
	[DefOf]
	public static class RaidAgeRestrictionDefOf
	{
		[MayRequireBiotech]
		public static RaidAgeRestrictionDef Children;

		static RaidAgeRestrictionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RaidAgeRestrictionDefOf));
		}
	}
}
