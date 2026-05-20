using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_EdgeWalkInHateChanters : PawnsArrivalModeWorker
{
	private static readonly HashSet<IntVec3> EntryCells = new HashSet<IntVec3>();

	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
		EntryCells.Clear();
		Map map = (Map)parms.target;
		int i;
		for (i = 0; i < pawns.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < 100; j++)
			{
				if (RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Ignore, allowFogged: false, SmallGroupValidator))
				{
					pawns[i].Position = result;
					EntryCells.Add(result);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		pawns.RemoveRange(i, pawns.Count - i);
		bool SmallGroupValidator(IntVec3 cell)
		{
			if (EntryCells.Count == 0)
			{
				return true;
			}
			if (pawns.Count < 20)
			{
				bool flag2 = false;
				foreach (IntVec3 entryCell in EntryCells)
				{
					if (cell.DistanceToSquared(entryCell) < 200)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					return !EntryCells.Contains(cell);
				}
				return false;
			}
			return !EntryCells.Contains(cell);
		}
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		parms.spawnRotation = Rot4.Random;
		return true;
	}
}
