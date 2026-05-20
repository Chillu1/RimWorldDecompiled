using Verse;

namespace RimWorld;

[DefOf]
public static class TerrainDefOf
{
	public static TerrainDef Sand;

	public static TerrainDef SoftSand;

	public static TerrainDef Soil;

	public static TerrainDef SoilRich;

	public static TerrainDef Underwall;

	public static TerrainDef Concrete;

	public static TerrainDef AncientConcrete;

	public static TerrainDef MetalTile;

	public static TerrainDef AncientTile;

	public static TerrainDef Gravel;

	public static TerrainDef WaterDeep;

	public static TerrainDef WaterShallow;

	public static TerrainDef WaterMovingChestDeep;

	public static TerrainDef WaterMovingShallow;

	public static TerrainDef WaterOceanDeep;

	public static TerrainDef WaterOceanShallow;

	public static TerrainDef PavedTile;

	public static TerrainDef WoodPlankFloor;

	public static TerrainDef TileSandstone;

	public static TerrainDef Ice;

	public static TerrainDef Marsh;

	public static TerrainDef Mud;

	public static TerrainDef FlagstoneSandstone;

	public static TerrainDef Bridge;

	public static TerrainDef Sandstone_Smooth;

	public static TerrainDef PackedDirt;

	public static TerrainDef BrokenAsphalt;

	public static TerrainDef Riverbank;

	[MayRequireIdeology]
	public static TerrainDef FungalGravel;

	[MayRequireAnomaly]
	public static TerrainDef Flesh;

	[MayRequireAnomaly]
	public static TerrainDef Voidmetal;

	[MayRequireAnomaly]
	public static TerrainDef GraySurface;

	[MayRequireOdyssey]
	public static TerrainDef Substructure;

	[MayRequireOdyssey]
	public static TerrainDef AncientMegastructure;

	[MayRequireOdyssey]
	public static TerrainDef LavaDeep;

	[MayRequireOdyssey]
	public static TerrainDef LavaShallow;

	[MayRequireOdyssey]
	public static TerrainDef CooledLava;

	[MayRequireOdyssey]
	public static TerrainDef VolcanicRock;

	[MayRequireOdyssey]
	public static TerrainDef ThinIce;

	[MayRequireOdyssey]
	public static TerrainDef ShallowFloodwater;

	[MayRequireOdyssey]
	public static TerrainDef MarshFlood;

	[MayRequireOdyssey]
	public static TerrainDef Space;

	[MayRequireOdyssey]
	public static TerrainDef ToxicWaterShallow;

	[MayRequireOdyssey]
	public static TerrainDef ToxicWaterDeep;

	[MayRequireOdyssey]
	public static TerrainDef InsectSludge;

	[MayRequireOdyssey]
	public static TerrainDef OrbitalPlatform;

	[MayRequireOdyssey]
	public static TerrainDef MechanoidPlatform;

	[MayRequireOdyssey]
	public static TerrainDef DryLakeBed;

	[MayRequireOdyssey]
	public static TerrainDef HotSpring;

	[MayRequireOdyssey]
	public static TerrainDef HeavyBridge;

	[MayRequireOdyssey]
	public static TerrainDef Vacstone_Rough;

	[MayRequireOdyssey]
	public static TerrainDef Vacstone_RoughHewn;

	static TerrainDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(TerrainDefOf));
	}
}
