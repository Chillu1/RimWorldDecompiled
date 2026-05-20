using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ResearchProjectTagDefOf
	{
		public static ResearchProjectTagDef ShipRelated;

		public static ResearchProjectTagDef ClassicStart;

		static ResearchProjectTagDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ResearchProjectTagDefOf));
		}
	}
}
