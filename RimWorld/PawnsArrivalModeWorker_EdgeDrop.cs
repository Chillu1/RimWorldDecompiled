using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnsArrivalModeWorker_EdgeDrop : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			PawnsArrivalModeWorkerUtility.DropInDropPodsNearSpawnCenter(parms, pawns);
		}

		public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			IntVec3 near = DropCellFinder.FindRaidDropCenterDistant(map);
			TransportPodsArrivalActionUtility.DropTravelingTransportPods(dropPods, near, map);
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
}
