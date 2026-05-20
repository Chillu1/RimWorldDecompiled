using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ClamorDefOf
	{
		public static ClamorDef Movement;

		public static ClamorDef Harm;

		public static ClamorDef Construction;

		public static ClamorDef Impact;

		public static ClamorDef Ability;

		[MayRequireBiotech]
		public static ClamorDef BabyCry;

		static ClamorDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ClamorDefOf));
		}
	}
}
