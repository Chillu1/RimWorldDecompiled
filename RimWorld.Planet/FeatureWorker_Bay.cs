using Verse;

namespace RimWorld.Planet;

public class FeatureWorker_Bay : FeatureWorker_Protrusion
{
	protected override bool IsRoot(PlanetTile tile)
	{
		BiomeDef primaryBiome = Find.WorldGrid[tile].PrimaryBiome;
		if (primaryBiome != BiomeDefOf.Ocean)
		{
			return primaryBiome == BiomeDefOf.Lake;
		}
		return true;
	}
}
