namespace RimWorld;

[DefOf]
public static class MapMeshFlagDefOf
{
	public static MapMeshFlagDef None;

	public static MapMeshFlagDef Things;

	public static MapMeshFlagDef FogOfWar;

	public static MapMeshFlagDef Buildings;

	public static MapMeshFlagDef GroundGlow;

	public static MapMeshFlagDef Terrain;

	public static MapMeshFlagDef Roofs;

	public static MapMeshFlagDef Snow;

	public static MapMeshFlagDef Zone;

	public static MapMeshFlagDef Plan;

	public static MapMeshFlagDef PowerGrid;

	public static MapMeshFlagDef BuildingsDamage;

	public static MapMeshFlagDef Gas;

	[MayRequireBiotech]
	public static MapMeshFlagDef Pollution;

	[MayRequireOdyssey]
	public static MapMeshFlagDef Sand;

	static MapMeshFlagDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(MapMeshFlagDefOf));
	}
}
