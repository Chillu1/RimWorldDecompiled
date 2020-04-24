namespace Verse
{
	public struct FloodFillRange
	{
		public int minX;

		public int maxX;

		public int z;

		public FloodFillRange(int minX, int maxX, int y)
		{
			this.minX = minX;
			this.maxX = maxX;
			z = y;
		}
	}
}
