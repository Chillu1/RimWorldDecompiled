using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_EdgeDrop : PawnsArrivalModeWorker
{
	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
		PawnsArrivalModeWorkerUtility.DropInDropPodsNearSpawnCenter(parms, pawns);
	}

	public override void TravellingTransportersArrived(List<ActiveTransporterInfo> transporters, Map map)
	{
		IntVec3 near = DropCellFinder.FindRaidDropCenterDistant(map, allowRoofed: false, !transporters.IsShuttle());
		if (transporters.IsShuttle())
		{
			TransportersArrivalActionUtility.DropShuttle(transporters[0], map, near);
		}
		else
		{
			TransportersArrivalActionUtility.DropTravellingDropPods(transporters, near, map);
		}
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!parms.spawnCenter.IsValid)
		{
			parms.spawnCenter = DropCellFinder.FindRaidDropCenterDistant(map);
		}
		parms.spawnRotation = Rot4.Random;
		return true;
	}
}
