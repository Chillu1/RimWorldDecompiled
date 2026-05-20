using RimWorld.Planet;

namespace RimWorld;

public class BiomeWorker_TemperateSwamp : BiomeWorker
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
		if (tile.swampiness < 0.5f)
		{
			return 0f;
		}
		return 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f + tile.swampiness * 3f;
	}
}
