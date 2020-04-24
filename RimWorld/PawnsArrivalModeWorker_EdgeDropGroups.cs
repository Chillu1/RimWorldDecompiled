using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnsArrivalModeWorker_EdgeDropGroups : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			Map map = (Map)parms.target;
			bool canRoofPunch = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
			List<Pair<List<Pawn>, IntVec3>> list = PawnsArrivalModeWorkerUtility.SplitIntoRandomGroupsNearMapEdge(pawns, map, arriveInPods: true);
			PawnsArrivalModeWorkerUtility.SetPawnGroupsInfo(parms, list);
			for (int i = 0; i < list.Count; i++)
			{
				DropPodUtility.DropThingsNear(list[i].Second, map, list[i].First.Cast<Thing>(), parms.podOpenDelay, canInstaDropDuringInit: false, leaveSlag: true, canRoofPunch);
			}
		}

		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			parms.spawnRotation = Rot4.Random;
			return true;
		}
	}
}
