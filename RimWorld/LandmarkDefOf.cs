namespace RimWorld;

[DefOf]
public static class LandmarkDefOf
{
	[MayRequireOdyssey]
	public static LandmarkDef Oasis;

	[MayRequireOdyssey]
	public static LandmarkDef Lake;

	[MayRequireOdyssey]
	public static LandmarkDef LakeWithIsland;

	[MayRequireOdyssey]
	public static LandmarkDef LakeWithIslands;

	[MayRequireOdyssey]
	public static LandmarkDef Pond;

	[MayRequireOdyssey]
	public static LandmarkDef DryLake;

	[MayRequireOdyssey]
	public static LandmarkDef ToxicLake;

	[MayRequireOdyssey]
	public static LandmarkDef Wetland;

	[MayRequireOdyssey]
	public static LandmarkDef HotSprings;

	[MayRequireOdyssey]
	public static LandmarkDef CoastalIsland;

	[MayRequireOdyssey]
	public static LandmarkDef Peninsula;

	[MayRequireOdyssey]
	public static LandmarkDef Valley;

	[MayRequireOdyssey]
	public static LandmarkDef Cavern;

	[MayRequireOdyssey]
	public static LandmarkDef Chasm;

	[MayRequireOdyssey]
	public static LandmarkDef IceDunes;

	[MayRequireOdyssey]
	public static LandmarkDef Cliffs;

	[MayRequireOdyssey]
	public static LandmarkDef Hollow;

	[MayRequireOdyssey]
	public static LandmarkDef TerraformingScar;

	[MayRequireOdyssey]
	public static LandmarkDef LavaFlow;

	[MayRequireOdyssey]
	public static LandmarkDef Dunes;

	[MayRequireOdyssey]
	public static LandmarkDef AncientSmokeVent;

	[MayRequireOdyssey]
	public static LandmarkDef AncientToxVent;

	[MayRequireOdyssey]
	public static LandmarkDef Ruins;

	static LandmarkDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(LandmarkDefOf));
	}
}
