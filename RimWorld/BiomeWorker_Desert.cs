using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_Desert : BiomeWorker
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
		if (tile.rainfall >= 600f)
		{
			return false;
		}
		return true;
	}

	public static float Score(Tile tile)
	{
		return tile.temperature + 0.0001f;
	}
}
