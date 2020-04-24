using System.Collections.Generic;

namespace Verse.AI
{
	public class PawnPathPool
	{
		private Map map;

		private List<PawnPath> paths = new List<PawnPath>(64);

		private static readonly PawnPath NotFoundPathInt;

		public static PawnPath NotFoundPath => NotFoundPathInt;

		public PawnPathPool(Map map)
		{
			this.map = map;
		}

		static PawnPathPool()
		{
			NotFoundPathInt = PawnPath.NewNotFound();
		}

		public PawnPath GetEmptyPawnPath()
		{
			for (int i = 0; i < paths.Count; i++)
			{
				if (!paths[i].inUse)
				{
					paths[i].inUse = true;
					return paths[i];
				}
			}
			if (paths.Count > map.mapPawns.AllPawnsSpawnedCount + 2)
			{
				Log.ErrorOnce("PawnPathPool leak: more paths than spawned pawns. Force-recovering.", 664788);
				paths.Clear();
			}
			PawnPath pawnPath = new PawnPath();
			paths.Add(pawnPath);
			pawnPath.inUse = true;
			return pawnPath;
		}
	}
}
