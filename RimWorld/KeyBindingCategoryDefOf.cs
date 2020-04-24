using Verse;

namespace RimWorld
{
	[DefOf]
	public static class KeyBindingCategoryDefOf
	{
		public static KeyBindingCategoryDef MainTabs;

		static KeyBindingCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingCategoryDefOf));
		}
	}
}
