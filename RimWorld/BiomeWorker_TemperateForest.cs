using RimWorld.Planet;

namespace RimWorld
{
	public class BiomeWorker_TemperateForest : BiomeWorker
	{
		public override float GetScore(Tile tile, int tileID)
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
			return 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f;
		}
	}
}
