using Verse;

namespace RimWorld;

public class CompProperties_PollutionPump : CompProperties, IPlantEffectRadius
{
	public float radius = 9.9f;

	public int intervalTicks = 60000;

	public EffecterDef pumpEffecterDef;

	public int pumpsPerWastepack = 3;

	public bool disabledByArtificialBuildings;

	public float PlantEffectRadius => radius;

	public CompProperties_PollutionPump()
	{
		compClass = typeof(CompPollutionPump);
	}
}
