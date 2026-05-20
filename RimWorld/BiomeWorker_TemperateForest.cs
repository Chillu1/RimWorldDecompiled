using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_TemperateForest : BiomeWorker
{
	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (tile.WaterCovered)
		{
			return -100f;
		}
		if (!Allowed(tile))
		{
			return 0f;
		}
		return Score(tile);
	}

	public static bool Allowed(Tile tile)
	{
		if (tile.temperature >= -10f)
		{
			return tile.rainfall >= 600f;
		}
		return false;
	}

	public static float Score(Tile tile)
	{
		return 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f;
	}
}
