namespace RimWorld
{
	[DefOf]
	public static class BiomeDefOf
	{
		public static BiomeDef IceSheet;

		public static BiomeDef Tundra;

		public static BiomeDef BorealForest;

		public static BiomeDef TemperateForest;

		public static BiomeDef TropicalRainforest;

		public static BiomeDef Desert;

		public static BiomeDef AridShrubland;

		public static BiomeDef SeaIce;

		public static BiomeDef Ocean;

		public static BiomeDef Lake;

		static BiomeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BiomeDefOf));
		}
	}
}
