using Verse;

namespace RimWorld
{
	[DefOf]
	public static class RoofDefOf
	{
		public static RoofDef RoofConstructed;

		public static RoofDef RoofRockThick;

		public static RoofDef RoofRockThin;

		static RoofDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RoofDefOf));
		}
	}
}
