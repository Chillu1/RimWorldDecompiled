namespace Verse
{
	public struct CachedTempInfo
	{
		public int roomGroupID;

		public int numCells;

		public float temperature;

		public static CachedTempInfo NewCachedTempInfo()
		{
			CachedTempInfo result = default(CachedTempInfo);
			result.Reset();
			return result;
		}

		public void Reset()
		{
			roomGroupID = -1;
			numCells = 0;
			temperature = 0f;
		}

		public CachedTempInfo(int roomGroupID, int numCells, float temperature)
		{
			this.roomGroupID = roomGroupID;
			this.numCells = numCells;
			this.temperature = temperature;
		}
	}
}
