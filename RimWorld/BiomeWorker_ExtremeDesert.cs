using RimWorld.Planet;

namespace RimWorld
{
	public class BiomeWorker_ExtremeDesert : BiomeWorker
	{
		public override float GetScore(Tile tile, int tileID)
		{
			if (tile.WaterCovered)
			{
				return -100f;
			}
			if (tile.rainfall >= 340f)
			{
				return 0f;
			}
			return tile.temperature * 2.7f - 13f - tile.rainfall * 0.14f;
		}
	}
}
