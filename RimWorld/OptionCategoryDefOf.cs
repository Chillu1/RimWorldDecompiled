using Verse;

namespace RimWorld
{
	[DefOf]
	public static class OptionCategoryDefOf
	{
		public static OptionCategoryDef General;

		public static OptionCategoryDef Graphics;

		public static OptionCategoryDef Audio;

		public static OptionCategoryDef Gameplay;

		public static OptionCategoryDef Interface;

		public static OptionCategoryDef Controls;

		public static OptionCategoryDef Dev;

		public static OptionCategoryDef Mods;

		static OptionCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(OptionCategoryDefOf));
		}
	}
}
