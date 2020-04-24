using Verse;

namespace RimWorld
{
	public class ConfiguredTicksAbsAtGameStartCache
	{
		private int cachedTicks = -1;

		private int cachedForStartingTile = -1;

		private Season cachedForStartingSeason;

		public bool TryGetCachedValue(GameInitData initData, out int ticksAbs)
		{
			if (initData.startingTile == cachedForStartingTile && initData.startingSeason == cachedForStartingSeason)
			{
				ticksAbs = cachedTicks;
				return true;
			}
			ticksAbs = -1;
			return false;
		}

		public void Cache(int ticksAbs, GameInitData initData)
		{
			cachedTicks = ticksAbs;
			cachedForStartingTile = initData.startingTile;
			cachedForStartingSeason = initData.startingSeason;
		}
	}
}
