using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ResearchProjectDefOf
	{
		public static ResearchProjectDef CarpetMaking;

		static ResearchProjectDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ResearchProjectDefOf));
		}
	}
}
