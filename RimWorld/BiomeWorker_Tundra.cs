using RimWorld.Planet;

namespace RimWorld
{
	public class BiomeWorker_Tundra : BiomeWorker
	{
		public override float GetScore(Tile tile, int tileID)
		{
			if (tile.WaterCovered)
			{
				return -100f;
			}
			return 0f - tile.temperature;
		}
	}
}
