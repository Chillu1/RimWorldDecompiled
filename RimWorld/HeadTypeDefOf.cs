using Verse;

namespace RimWorld
{
	[DefOf]
	public static class HeadTypeDefOf
	{
		public static HeadTypeDef Skull;

		public static HeadTypeDef Stump;

		static HeadTypeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HeadTypeDefOf));
		}
	}
}
