using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_Biome : FeatureWorker_FloodFill
	{
		protected override bool IsRoot(int tile)
		{
			return def.rootBiomes.Contains(Find.WorldGrid[tile].biome);
		}

		protected override bool IsPossiblyAllowed(int tile)
		{
			return def.acceptableBiomes.Contains(Find.WorldGrid[tile].biome);
		}
	}
}
