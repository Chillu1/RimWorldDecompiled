using Verse;

namespace RimWorld.Planet
{
	public class WorldReachability
	{
		private int[] fields;

		private int nextFieldID;

		private int impassableFieldID;

		private int minValidFieldID;

		public WorldReachability()
		{
			fields = new int[Find.WorldGrid.TilesCount];
			nextFieldID = 1;
			InvalidateAllFields();
		}

		public void ClearCache()
		{
			InvalidateAllFields();
		}

		public bool CanReach(Caravan c, int tile)
		{
			return CanReach(c.Tile, tile);
		}

		public bool CanReach(int startTile, int destTile)
		{
			if (startTile < 0 || startTile >= fields.Length || destTile < 0 || destTile >= fields.Length)
			{
				return false;
			}
			if (fields[startTile] == impassableFieldID || fields[destTile] == impassableFieldID)
			{
				return false;
			}
			if (IsValidField(fields[startTile]) || IsValidField(fields[destTile]))
			{
				return fields[startTile] == fields[destTile];
			}
			FloodFillAt(startTile);
			if (fields[startTile] == impassableFieldID)
			{
				return false;
			}
			return fields[startTile] == fields[destTile];
		}

		private void InvalidateAllFields()
		{
			if (nextFieldID == 2147483646)
			{
				nextFieldID = 1;
			}
			minValidFieldID = nextFieldID;
			impassableFieldID = nextFieldID;
			nextFieldID++;
		}

		private bool IsValidField(int fieldID)
		{
			return fieldID >= minValidFieldID;
		}

		private void FloodFillAt(int tile)
		{
			World world = Find.World;
			if (world.Impassable(tile))
			{
				fields[tile] = impassableFieldID;
				return;
			}
			Find.WorldFloodFiller.FloodFill(tile, (int x) => !world.Impassable(x), delegate(int x)
			{
				fields[x] = nextFieldID;
			});
			nextFieldID++;
		}
	}
}
