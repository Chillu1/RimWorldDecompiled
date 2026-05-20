namespace RimWorld;

[DefOf]
public static class BiomeDefOf
{
	public static BiomeDef IceSheet;

	public static BiomeDef Tundra;

	public static BiomeDef BorealForest;

	public static BiomeDef TemperateForest;

	public static BiomeDef Desert;

	public static BiomeDef SeaIce;

	public static BiomeDef Ocean;

	public static BiomeDef Lake;

	public static BiomeDef ColdBog;

	public static BiomeDef TropicalRainforest;

	public static BiomeDef TropicalSwamp;

	[MayRequireAnomaly]
	public static BiomeDef Undercave;

	[MayRequireOdyssey]
	public static BiomeDef Scarlands;

	[MayRequireOdyssey]
	public static BiomeDef Space;

	[MayRequireOdyssey]
	public static BiomeDef Orbit;

	[MayRequireOdyssey]
	public static BiomeDef Glowforest;

	static BiomeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(BiomeDefOf));
	}
}
