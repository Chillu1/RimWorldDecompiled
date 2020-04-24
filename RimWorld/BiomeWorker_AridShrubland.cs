using RimWorld.Planet;

namespace RimWorld
{
	public class BiomeWorker_AridShrubland : BiomeWorker
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
			if (tile.rainfall < 600f || tile.rainfall >= 2000f)
			{
				return 0f;
			}
			return 22.5f + (tile.temperature - 20f) * 2.2f + (tile.rainfall - 600f) / 100f;
		}
	}
}
