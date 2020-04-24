using RimWorld.Planet;

namespace RimWorld
{
	public class BiomeWorker_Desert : BiomeWorker
	{
		public override float GetScore(Tile tile, int tileID)
		{
			if (tile.WaterCovered)
			{
				return -100f;
			}
			if (tile.rainfall >= 600f)
			{
				return 0f;
			}
			return tile.temperature + 0.0001f;
		}
	}
}
