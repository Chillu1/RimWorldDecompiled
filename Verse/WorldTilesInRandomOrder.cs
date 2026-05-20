using System.Collections.Generic;

namespace Verse
{
	public class WorldTilesInRandomOrder
	{
		private List<int> randomizedTiles;

		public List<int> Tiles
		{
			get
			{
				if (randomizedTiles == null)
				{
					randomizedTiles = new List<int>();
					for (int i = 0; i < Find.WorldGrid.TilesCount; i++)
					{
						randomizedTiles.Add(i);
					}
					Rand.PushState();
					Rand.Seed = Find.World.info.Seed;
					randomizedTiles.Shuffle();
					Rand.PopState();
				}
				return randomizedTiles;
			}
		}
	}
}
