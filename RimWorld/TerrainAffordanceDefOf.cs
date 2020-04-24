using Verse;

namespace RimWorld
{
	[DefOf]
	public static class TerrainAffordanceDefOf
	{
		public static TerrainAffordanceDef Light;

		public static TerrainAffordanceDef Medium;

		public static TerrainAffordanceDef Heavy;

		public static TerrainAffordanceDef GrowSoil;

		public static TerrainAffordanceDef Diggable;

		public static TerrainAffordanceDef SmoothableStone;

		public static TerrainAffordanceDef MovingFluid;

		public static TerrainAffordanceDef Bridgeable;

		static TerrainAffordanceDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TerrainAffordanceDefOf));
		}
	}
}
