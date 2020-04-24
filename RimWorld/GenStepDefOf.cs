using Verse;

namespace RimWorld
{
	[DefOf]
	public static class GenStepDefOf
	{
		public static GenStepDef PreciousLump;

		static GenStepDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(GenStepDefOf));
		}
	}
}
