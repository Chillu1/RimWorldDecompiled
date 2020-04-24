using Verse;

namespace RimWorld
{
	[DefOf]
	public static class TerrainDefOf
	{
		public static TerrainDef Sand;

		public static TerrainDef Soil;

		public static TerrainDef Underwall;

		public static TerrainDef Concrete;

		public static TerrainDef MetalTile;

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

		public static TerrainDef FlagstoneSandstone;

		public static TerrainDef Bridge;

		static TerrainDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TerrainDefOf));
		}
	}
}
