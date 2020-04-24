using Verse;

namespace RimWorld
{
	[DefOf]
	public static class SubcameraDefOf
	{
		public static SubcameraDef WaterDepth;

		static SubcameraDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SubcameraDefOf));
		}
	}
}
