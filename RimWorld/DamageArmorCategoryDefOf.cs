using Verse;

namespace RimWorld
{
	[DefOf]
	public static class DamageArmorCategoryDefOf
	{
		public static DamageArmorCategoryDef Sharp;

		static DamageArmorCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DamageArmorCategoryDefOf));
		}
	}
}
