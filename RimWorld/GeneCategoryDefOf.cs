using Verse;

namespace RimWorld
{
	[DefOf]
	public static class GeneCategoryDefOf
	{
		[MayRequireBiotech]
		public static GeneCategoryDef Archite;

		public static GeneCategoryDef Miscellaneous;

		static GeneCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GeneCategoryDefOf));
		}
	}
}
