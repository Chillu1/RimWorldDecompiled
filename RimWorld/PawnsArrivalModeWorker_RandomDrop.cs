using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnsArrivalModeWorker_RandomDrop : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			Map map = (Map)parms.target;
			bool canRoofPunch = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
			for (int i = 0; i < pawns.Count; i++)
			{
				DropPodUtility.DropThingsNear(DropCellFinder.RandomDropSpot(map), map, Gen.YieldSingle((Thing)pawns[i]), parms.podOpenDelay, canInstaDropDuringInit: false, leaveSlag: true, canRoofPunch);
			}
		}

		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			if (!parms.raidArrivalModeForQuickMilitaryAid)
			{
				parms.podOpenDelay = 520;
			}
			parms.spawnRotation = Rot4.Random;
			return true;
		}
	}
}
