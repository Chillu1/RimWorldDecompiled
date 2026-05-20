using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_Ocean : BiomeWorker
{
	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (!tile.WaterCovered)
		{
			return -100f;
		}
		return 0f;
	}
}
