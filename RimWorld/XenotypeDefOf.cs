namespace RimWorld
{
	[DefOf]
	public static class XenotypeDefOf
	{
		[MayRequireBiotech]
		public static XenotypeDef Baseliner;

		[MayRequireBiotech]
		public static XenotypeDef Sanguophage;

		static XenotypeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(XenotypeDefOf));
		}
	}
}
