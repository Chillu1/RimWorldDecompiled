using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_Glowforest : BiomeWorker
{
	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (tile.WaterCovered)
		{
			return -100f;
		}
		if (tile.temperature < 10f || tile.temperature > 55f)
		{
			return 0f;
		}
		if (tile.rainfall < 1000f)
		{
			return 0f;
		}
		if (tile.swampiness < 0.6f)
		{
			return 0f;
		}
		return 23f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 220f + tile.swampiness * 3f;
	}
}
