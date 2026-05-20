using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_EdgeWalkInDistributed : PawnsArrivalModeWorker
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
				if (RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Ignore))
				{
					pawns[i].Position = result;
					EntryCells.Add(result);
					flag = true;
					GenSpawn.Spawn(pawns[i], result, map, parms.spawnRotation);
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		pawns.RemoveRange(i, pawns.Count - i);
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		parms.spawnRotation = Rot4.Random;
		return true;
	}
}
