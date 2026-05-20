using Verse;

namespace RimWorld.Planet;

public class FeatureWorker_Biome : FeatureWorker_FloodFill
{
	protected override bool IsRoot(PlanetTile tile)
	{
		return def.rootBiomes.Contains(Find.WorldGrid[tile].PrimaryBiome);
	}

	protected override bool IsPossiblyAllowed(PlanetTile tile)
	{
		return def.acceptableBiomes.Contains(Find.WorldGrid[tile].PrimaryBiome);
	}
}
