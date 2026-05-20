using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_TropicalSwamp : BiomeWorker
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
		if (tile.temperature >= 15f && tile.temperature <= 55f && tile.rainfall >= 2000f)
		{
			return tile.swampiness >= 0.5f;
		}
		return false;
	}

	public static float Score(Tile tile)
	{
		return 28f + (tile.temperature - 20f) * 1.5f + (tile.rainfall - 600f) / 165f + tile.swampiness * 3f;
	}
}
