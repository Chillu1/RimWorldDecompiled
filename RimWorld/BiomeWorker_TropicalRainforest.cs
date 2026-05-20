using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_TropicalRainforest : BiomeWorker
{
	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (tile.WaterCovered)
		{
			return -100f;
		}
		if (tile.temperature < 15f)
		{
			return 0f;
		}
		if (tile.rainfall < 2000f)
		{
			return 0f;
		}
		return 28f + (tile.temperature - 20f) * 1.5f + (tile.rainfall - 600f) / 165f;
	}
}
