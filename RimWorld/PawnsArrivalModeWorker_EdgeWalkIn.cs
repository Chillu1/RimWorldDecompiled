using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class PawnsArrivalModeWorker_EdgeWalkIn : PawnsArrivalModeWorker
{
	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
		Map map = (Map)parms.target;
		for (int i = 0; i < pawns.Count; i++)
		{
			IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 8);
			GenSpawn.Spawn(pawns[i], loc, map, parms.spawnRotation);
		}
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (parms.attackTargets != null && parms.attackTargets.Count > 0 && !RCellFinder.TryFindEdgeCellFromThingAvoidingColony(parms.attackTargets[0], map, predicate, out parms.spawnCenter))
		{
			CellFinder.TryFindRandomEdgeCellWith((IntVec3 p) => (map.TileInfo.AllowRoofedEdgeWalkIn || !map.roofGrid.Roofed(p)) && p.Walkable(map), map, CellFinder.EdgeRoadChance_Hostile, out parms.spawnCenter);
		}
		if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Hostile))
		{
			return false;
		}
		parms.spawnRotation = Rot4.FromAngleFlat((map.Center - parms.spawnCenter).AngleFlat);
		return true;
		bool predicate(IntVec3 from, IntVec3 to)
		{
			if ((map.TileInfo.AllowRoofedEdgeWalkIn || !map.roofGrid.Roofed(from)) && from.Walkable(map))
			{
				return map.reachability.CanReach(from, to, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some);
			}
			return false;
		}
	}
}
