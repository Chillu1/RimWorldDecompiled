using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_IceSheet : BiomeWorker
{
	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (tile.WaterCovered)
		{
			return -100f;
		}
		return PermaIceScore(tile);
	}

	public static float PermaIceScore(Tile tile)
	{
		return -20f + (0f - tile.temperature) * 2f;
	}
}
