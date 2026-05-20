using Verse;

namespace RimWorld;

public class BiomeConditionMapComponent : MapComponent
{
	public BiomeConditionMapComponent(Map map)
		: base(map)
	{
	}

	public override void MapGenerated()
	{
		if (map.Biome.biomeMapConditions.NullOrEmpty())
		{
			return;
		}
		foreach (GameConditionDef biomeMapCondition in map.Biome.biomeMapConditions)
		{
			map.GameConditionManager.RegisterCondition(GameConditionMaker.MakeConditionPermanent(biomeMapCondition));
		}
	}
}
