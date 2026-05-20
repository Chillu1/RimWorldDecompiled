using Verse;

namespace RimWorld.Planet;

public class FeatureWorker_MountainRange : FeatureWorker_Cluster
{
	protected override bool IsRoot(PlanetTile tile)
	{
		return Find.WorldGrid[tile].hilliness != Hilliness.Flat;
	}

	protected override bool CanTraverse(PlanetTile tile, out bool ifRootThenRootGroupSizeMustMatch)
	{
		ifRootThenRootGroupSizeMustMatch = false;
		return Find.WorldGrid[tile].PrimaryBiome != BiomeDefOf.Ocean;
	}
}
