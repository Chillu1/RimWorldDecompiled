using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_Archipelago : FeatureWorker_Cluster
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

		protected override bool CanTraverse(int tile, out bool ifRootThenRootGroupSizeMustMatch)
		{
			ifRootThenRootGroupSizeMustMatch = true;
			return true;
		}

		protected override bool IsMember(int tile, out bool ifRootThenRootGroupSizeMustMatch)
		{
			ifRootThenRootGroupSizeMustMatch = true;
			bool ifRootThenRootGroupSizeMustMatch2;
			return base.IsMember(tile, out ifRootThenRootGroupSizeMustMatch2);
		}
	}
}
