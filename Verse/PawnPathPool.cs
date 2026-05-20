using System.Collections.Generic;
using Verse.AI;

namespace Verse;

public class PawnPathPool
{
	private readonly Map map;

	private int pathsCreated;

	private readonly Stack<PawnPath> paths = new Stack<PawnPath>(64);

	public PawnPathPool(Map map)
	{
		this.map = map;
	}

	public PawnPath GetPath()
	{
		if (!paths.TryPop(out var result))
		{
			result = new PawnPath();
			result.pool = this;
			int num = map.mapPawns.AllPawnsSpawnedCount * 2 + 1 + 4;
			if (++pathsCreated > num)
			{
				Log.ErrorOnce(string.Format("Leak suspected in object pool for {0}s, created: {1}, expected less than {2}. Dispose them so they can be reused by the {3}.", "PawnPath", pathsCreated, num, "PawnPathPool"), 664788);
			}
		}
		return result;
	}

	public void ReturnPath(PawnPath path)
	{
		paths.Push(path);
	}
}
