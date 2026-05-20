using Verse;

namespace RimWorld;

public class CompProperties_GrowsFleshmassTendrils : CompProperties
{
	public int mtbGrowthCycleHours;

	public int minGrowthCycleHours;

	public int maxGrowthCycleHours;

	public SimpleCurve startingGrowthPointsByThreat;

	public SimpleCurve growthCyclePointsByThreat;

	public SimpleCurve fleshbeastPointsByThreat;

	public IntRange? fleshbeastBirthThresholdRange;

	public bool spawnFleshbeasts = true;

	public CompProperties_GrowsFleshmassTendrils()
	{
		compClass = typeof(CompGrowsFleshmassTendrils);
	}
}
