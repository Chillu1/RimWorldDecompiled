using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_MountainRange : FeatureWorker_Cluster
	{
		protected override bool IsRoot(int tile)
		{
			return Find.WorldGrid[tile].hilliness != Hilliness.Flat;
		}

		protected override bool CanTraverse(int tile, out bool ifRootThenRootGroupSizeMustMatch)
		{
			ifRootThenRootGroupSizeMustMatch = false;
			return Find.WorldGrid[tile].biome != BiomeDefOf.Ocean;
		}
	}
}
