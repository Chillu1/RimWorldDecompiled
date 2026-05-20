using Verse;

namespace RimWorld.Planet;

public class FeatureWorker_Island : FeatureWorker_FloodFill
{
	protected override bool IsRoot(PlanetTile tile)
	{
		BiomeDef primaryBiome = Find.WorldGrid[tile].PrimaryBiome;
		if (primaryBiome != BiomeDefOf.Ocean)
		{
			return primaryBiome != BiomeDefOf.Lake;
		}
		return false;
	}

	protected override bool IsPossiblyAllowed(PlanetTile tile)
	{
		return Find.WorldGrid[tile].PrimaryBiome == BiomeDefOf.Lake;
	}
}
