using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class PawnsArrivalModeWorker_Shuttle : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
		}

		public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			if (dropPods.Count > 1)
			{
				TransportPodsArrivalActionUtility.DropShuttle_NewTemp(dropPods, map, IntVec3.Invalid);
				return;
			}
			ActiveDropPodInfo activeDropPodInfo = dropPods[0];
			List<Pawn> requiredPawns = activeDropPodInfo.innerContainer.Where((Thing t) => t is Pawn).Cast<Pawn>().ToList();
			Thing thing = TransportPodsArrivalActionUtility.DropShuttle_NewTemp(dropPods, map, IntVec3.Invalid);
			thing.questTags = activeDropPodInfo.questTags;
			CompShuttle compShuttle = thing.TryGetComp<CompShuttle>();
			if (compShuttle == null)
			{
				return;
			}
			compShuttle.sendAwayIfQuestFinished = activeDropPodInfo.sendAwayIfQuestFinished;
			if (activeDropPodInfo.missionShuttleHome == null && activeDropPodInfo.missionShuttleTarget == null)
			{
				return;
			}
			compShuttle.missionShuttleTarget = activeDropPodInfo.missionShuttleHome;
			compShuttle.missionShuttleHome = null;
			compShuttle.stayAfterDroppedEverythingOnArrival = true;
			compShuttle.requiredPawns = requiredPawns;
			compShuttle.hideControls = false;
			if (compShuttle.missionShuttleTarget != null)
			{
				return;
			}
			foreach (Thing item in (IEnumerable<Thing>)compShuttle.Transporter.innerContainer)
			{
				Pawn pawn;
				if ((pawn = item as Pawn) != null && pawn.IsColonist)
				{
					pawn.inventory.UnloadEverything = true;
				}
			}
		}

		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			return true;
		}
	}
}
