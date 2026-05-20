using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_CenterDrop : PawnsArrivalModeWorker
{
	public const int PodOpenDelay = 520;

	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
		PawnsArrivalModeWorkerUtility.DropInDropPodsNearSpawnCenter(parms, pawns);
	}

	public override void TravellingTransportersArrived(List<ActiveTransporterInfo> transporters, Map map)
	{
		if (!DropCellFinder.TryFindRaidDropCenterClose(out var spot, map))
		{
			spot = DropCellFinder.FindRaidDropCenterDistant(map);
		}
		if (transporters.IsShuttle())
		{
			TransportersArrivalActionUtility.DropShuttle(transporters[0], map, spot);
		}
		else
		{
			TransportersArrivalActionUtility.DropTravellingDropPods(transporters, spot, map);
		}
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!parms.raidArrivalModeForQuickMilitaryAid)
		{
			parms.podOpenDelay = 520;
		}
		parms.spawnRotation = Rot4.Random;
		if (!parms.spawnCenter.IsValid)
		{
			bool flag = parms.faction != null && parms.faction == Faction.OfMechanoids;
			bool flag2 = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
			if (Rand.Chance(0.4f) && !flag && map.listerBuildings.ColonistsHaveBuildingWithPowerOn(ThingDefOf.OrbitalTradeBeacon))
			{
				parms.spawnCenter = DropCellFinder.TradeDropSpot(map);
			}
			else if (!DropCellFinder.TryFindRaidDropCenterClose(out parms.spawnCenter, map, !flag && flag2, !flag))
			{
				parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
				return parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
			}
		}
		return true;
	}
}
