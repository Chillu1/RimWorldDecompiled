using Verse;

namespace RimWorld
{
	[DefOf]
	public static class TipSetDefOf
	{
		public static TipSetDef GameplayTips;

		static TipSetDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TipSetDefOf));
		}
	}
}
