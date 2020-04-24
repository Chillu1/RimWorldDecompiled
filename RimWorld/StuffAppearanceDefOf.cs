using Verse;

namespace RimWorld
{
	[DefOf]
	public static class StuffAppearanceDefOf
	{
		public static StuffAppearanceDef Smooth;

		static StuffAppearanceDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(StuffAppearanceDefOf));
		}
	}
}
