using Verse;

namespace RimWorld
{
	[DefOf]
	public static class ResearchProjectTagDefOf
	{
		public static ResearchProjectTagDef ShipRelated;

		static ResearchProjectTagDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ResearchProjectTagDefOf));
		}
	}
}
