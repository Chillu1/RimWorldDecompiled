using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_Island : FeatureWorker_FloodFill
	{
		protected override bool IsRoot(int tile)
		{
			BiomeDef biome = Find.WorldGrid[tile].biome;
			if (biome != BiomeDefOf.Ocean)
			{
				return biome != BiomeDefOf.Lake;
			}
			return false;
		}

		protected override bool IsPossiblyAllowed(int tile)
		{
			return Find.WorldGrid[tile].biome == BiomeDefOf.Lake;
		}
	}
}
