using Verse;

namespace RimWorld;

public class CompProperties_DestroyNearbyPlantsOnSpawn : CompProperties
{
	public int radius;

	public CompProperties_DestroyNearbyPlantsOnSpawn()
	{
		compClass = typeof(CompDestroyNearbyPlantsOnSpawn);
	}
}
