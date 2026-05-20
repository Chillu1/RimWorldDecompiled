using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_Grasslands : BiomeWorker
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
		if (tile.temperature < -10f || tile.temperature > 55f)
		{
			return false;
		}
		if (tile.rainfall < 800f || tile.rainfall >= 2500f)
		{
			return false;
		}
		if (tile.hilliness == Hilliness.Mountainous || tile.hilliness == Hilliness.Impassable)
		{
			return false;
		}
		return true;
	}

	public static float Score(Tile tile)
	{
		return 22.5f + (tile.temperature - 20f) * 2.4f + (tile.rainfall - 800f) / 100f;
	}
}
