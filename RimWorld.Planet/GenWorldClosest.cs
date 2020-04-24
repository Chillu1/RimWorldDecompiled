using System;
using Verse;

namespace RimWorld.Planet
{
	public static class GenWorldClosest
	{
		public static bool TryFindClosestTile(int rootTile, Predicate<int> predicate, out int foundTile, int maxTilesToScan = int.MaxValue, bool canSearchThroughImpassable = true)
		{
			int foundTileLocal = -1;
			Find.WorldFloodFiller.FloodFill(rootTile, (int x) => canSearchThroughImpassable || !Find.World.Impassable(x), delegate(int t)
			{
				bool num = predicate(t);
				if (num)
				{
					foundTileLocal = t;
				}
				return num;
			}, maxTilesToScan);
			foundTile = foundTileLocal;
			return foundTileLocal >= 0;
		}

		public static bool TryFindClosestPassableTile(int rootTile, out int foundTile)
		{
			return TryFindClosestTile(rootTile, (int x) => !Find.World.Impassable(x), out foundTile);
		}
	}
}
