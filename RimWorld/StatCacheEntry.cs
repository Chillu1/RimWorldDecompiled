namespace RimWorld;

public struct StatCacheEntry
{
	public float statValue;

	public int gameTick;

	public StatCacheEntry(float statValue, int gameTick)
	{
		this.statValue = statValue;
		this.gameTick = gameTick;
	}

	public override string ToString()
	{
		return $"StatCacheEntry({statValue}, {gameTick})";
	}

	public override int GetHashCode()
	{
		return statValue.GetHashCode() ^ gameTick.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is StatCacheEntry statCacheEntry)
		{
			return Equals(statCacheEntry);
		}
		return false;
	}
}
