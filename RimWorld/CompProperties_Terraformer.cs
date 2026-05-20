using Verse;

namespace RimWorld;

public class CompProperties_Terraformer : CompProperties, IPlantEffectRadius
{
	public float radius = 6.9f;

	public float secondsPerConvert = 20f;

	public TerrainDef convertTerrainDef;

	public float PlantEffectRadius => radius;

	public CompProperties_Terraformer()
	{
		compClass = typeof(CompTerraformer);
	}
}
