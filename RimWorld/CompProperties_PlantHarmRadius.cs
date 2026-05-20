using Verse;

namespace RimWorld;

public class CompProperties_PlantHarmRadius : CompProperties
{
	public float harmFrequencyPerArea = 0.011f;

	public float leaflessPlantKillChance = 0.05f;

	public bool ignoreSpecialTrees;

	public bool messageOnCropDeath = true;

	public SimpleCurve radiusPerDayCurve;

	public CompProperties_PlantHarmRadius()
	{
		compClass = typeof(CompPlantHarmRadius);
	}
}
