namespace RimWorld;

[DefOf]
public static class WeaponCategoryDefOf
{
	[MayRequireRoyalty]
	public static WeaponCategoryDef BladeLink;

	static WeaponCategoryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(WeaponCategoryDefOf));
	}
}
