using RimWorld.Planet;

namespace RimWorld
{
	public abstract class BiomeWorker
	{
		public abstract float GetScore(Tile tile, int tileID);
	}
}
