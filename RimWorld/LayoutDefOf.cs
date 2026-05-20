namespace RimWorld;

[DefOf]
public static class LayoutDefOf
{
	[MayRequireIdeology]
	public static LayoutDef AncientComplex;

	[MayRequireIdeology]
	public static LayoutDef AncientComplex_Loot;

	[MayRequireBiotech]
	public static LayoutDef AncientComplex_Mechanitor_Loot;

	[MayRequireAnomaly]
	public static LayoutDef Labyrinth;

	[MayRequireOdyssey]
	public static LayoutDef AncientRuins_Scarlands;

	[MayRequireOdyssey]
	public static LayoutDef AncientRuins_Special;

	[MayRequireOdyssey]
	public static LayoutDef AncientRuinsGlacier;

	[MayRequireOdyssey]
	public static LayoutDef AncientRuinsReactor_Standard;

	[MayRequireOdyssey]
	public static LayoutDef AncientRuinsReactor_Reactor;

	[MayRequireOdyssey]
	public static LayoutDef AncientRuinsFrozenTerraformer;

	[MayRequireOdyssey]
	public static LayoutDef AncientStockpile;

	[MayRequireOdyssey]
	public static LayoutDef AncientStockpileFake;

	[MayRequireOdyssey]
	public static LayoutDef SpaceRuins;

	[MayRequireOdyssey]
	public static LayoutDef OrbitalMechanoidPlatform;

	[MayRequireOdyssey]
	public static LayoutDef OrbitalAncientPlatform;

	[MayRequireOdyssey]
	public static LayoutDef Mechhive;

	[MayRequireOdyssey]
	public static LayoutDef Mechhive_SingleRoom;

	[MayRequireOdyssey]
	public static LayoutDef OrbitalItemStash;

	[MayRequireOdyssey]
	public static LayoutDef CrashedMechanoidPlatform_Standard;

	[MayRequireOdyssey]
	public static LayoutDef CrashedMechanoidPlatform_Engine;

	static LayoutDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(LayoutDefOf));
	}
}
