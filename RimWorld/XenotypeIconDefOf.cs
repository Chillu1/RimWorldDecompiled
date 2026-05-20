namespace RimWorld
{
	[DefOf]
	public static class XenotypeIconDefOf
	{
		[MayRequireBiotech]
		public static XenotypeIconDef Basic;

		static XenotypeIconDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(XenotypeIconDefOf));
		}
	}
}
