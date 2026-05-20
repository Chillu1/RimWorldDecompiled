namespace Verse;

public struct CachedTempInfo
{
	public int roomID;

	public int numCells;

	public float temperature;

	public float vacuum;

	public static CachedTempInfo NewCachedTempInfo()
	{
		CachedTempInfo result = default(CachedTempInfo);
		result.Reset();
		return result;
	}

	public void Reset()
	{
		roomID = -1;
		numCells = 0;
		temperature = 0f;
		vacuum = 0f;
	}

	public CachedTempInfo(int roomID, int numCells, float temperature, float vacuum)
	{
		this.roomID = roomID;
		this.numCells = numCells;
		this.temperature = temperature;
		this.vacuum = vacuum;
	}
}
