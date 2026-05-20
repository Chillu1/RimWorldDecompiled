using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_BorealForest : BiomeWorker
{
	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (tile.WaterCovered)
		{
			return -100f;
		}
		if (tile.temperature < -10f)
		{
			return 0f;
		}
		if (tile.rainfall < 600f)
		{
			return 0f;
		}
		return 15f;
	}
}
