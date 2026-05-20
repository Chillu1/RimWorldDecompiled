using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_AridShrubland : BiomeWorker
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
		if (tile.temperature < -10f)
		{
			return false;
		}
		if (tile.rainfall < 600f || tile.rainfall >= 2000f)
		{
			return false;
		}
		return true;
	}

	public static float Score(Tile tile)
	{
		return 22.5f + (tile.temperature - 20f) * 2.2f + (tile.rainfall - 600f) / 100f;
	}
}
