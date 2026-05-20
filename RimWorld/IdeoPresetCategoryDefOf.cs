namespace RimWorld
{
	[DefOf]
	public static class IdeoPresetCategoryDefOf
	{
		[MayRequireIdeology]
		public static IdeoPresetCategoryDef Classic;

		[MayRequireIdeology]
		public static IdeoPresetCategoryDef Custom;

		[MayRequireIdeology]
		public static IdeoPresetCategoryDef Fluid;

		static IdeoPresetCategoryDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(IdeoPresetCategoryDefOf));
		}
	}
}
