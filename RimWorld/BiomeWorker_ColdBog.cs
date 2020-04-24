using RimWorld.Planet;

namespace RimWorld
{
	public class BiomeWorker_ColdBog : BiomeWorker
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
			if (tile.swampiness < 0.5f)
			{
				return 0f;
			}
			return 0f - tile.temperature + 13f + tile.swampiness * 8f;
		}
	}
}
