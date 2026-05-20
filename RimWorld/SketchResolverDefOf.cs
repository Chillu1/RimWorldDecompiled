namespace RimWorld;

[DefOf]
public static class SketchResolverDefOf
{
	public static SketchResolverDef Monument;

	public static SketchResolverDef MonumentRuin;

	public static SketchResolverDef Symmetry;

	public static SketchResolverDef AssignRandomStuff;

	public static SketchResolverDef FloorFill;

	public static SketchResolverDef AddColumns;

	public static SketchResolverDef AddCornerThings;

	public static SketchResolverDef AddThingsCentral;

	public static SketchResolverDef AddWallEdgeThings;

	public static SketchResolverDef AddInnerMonuments;

	public static SketchResolverDef DamageBuildings;

	public static SketchResolverDef DamageBuildingsLight;

	[MayRequireRoyalty]
	public static SketchResolverDef MechCluster;

	[MayRequireRoyalty]
	public static SketchResolverDef MechClusterWalls;

	[MayRequireIdeology]
	public static SketchResolverDef AncientUtilityBuilding;

	[MayRequireIdeology]
	public static SketchResolverDef AncientLandingPad;

	[MayRequireOdyssey]
	public static SketchResolverDef AncientHatch;

	[MayRequireOdyssey]
	public static SketchResolverDef Gravship;

	[MayRequireOdyssey]
	public static SketchResolverDef AncientConcretePad;

	[MayRequireOdyssey]
	public static SketchResolverDef CerebrexCore;

	[MayRequireOdyssey]
	public static SketchResolverDef MechhiveGuardOutpost;

	[MayRequireOdyssey]
	public static SketchResolverDef MechhiveLaunchPad;

	[MayRequireOdyssey]
	public static SketchResolverDef MechhiveProcessorArray;

	static SketchResolverDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(SketchResolverDefOf));
	}
}
