using Verse;

namespace RimWorld.Planet;

public class FeatureWorker_Archipelago : FeatureWorker_Cluster
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

	protected override bool CanTraverse(PlanetTile tile, out bool ifRootThenRootGroupSizeMustMatch)
	{
		ifRootThenRootGroupSizeMustMatch = true;
		return true;
	}

	protected override bool IsMember(PlanetTile tile, out bool ifRootThenRootGroupSizeMustMatch)
	{
		ifRootThenRootGroupSizeMustMatch = true;
		bool ifRootThenRootGroupSizeMustMatch2;
		return base.IsMember(tile, out ifRootThenRootGroupSizeMustMatch2);
	}
}
