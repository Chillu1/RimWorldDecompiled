using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_ClusterDrop : PawnsArrivalModeWorker
{
	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
	}

	public override void TravellingTransportersArrived(List<ActiveTransporterInfo> transporters, Map map)
	{
		IntVec3 near = DropCellFinder.FindRaidDropCenterDistant(map);
		TransportersArrivalActionUtility.DropTravellingDropPods(transporters, near, map);
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!parms.spawnCenter.IsValid)
		{
			parms.spawnCenter = MechClusterUtility.FindClusterPosition(map, parms.mechClusterSketch, 100, 0.5f);
		}
		parms.spawnRotation = Rot4.Random;
		return true;
	}
}
