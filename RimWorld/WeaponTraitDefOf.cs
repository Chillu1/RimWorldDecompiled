namespace RimWorld;

[DefOf]
public static class WeaponTraitDefOf
{
	[MayRequireRoyalty]
	public static WeaponTraitDef NeedKill;

	static WeaponTraitDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(WeaponTraitDefOf));
	}
}
