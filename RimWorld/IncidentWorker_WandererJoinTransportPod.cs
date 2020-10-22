using Verse;

namespace RimWorld
{
	public class IncidentWorker_WandererJoinTransportPod : IncidentWorker_WandererJoin
	{
		public override bool CanSpawnJoiner(Map map)
		{
			return true;
		}

		public override void SpawnJoiner(Map map, Pawn pawn)
		{
			IntVec3 c = DropCellFinder.RandomDropSpot(map);
			ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
			activeDropPodInfo.innerContainer.TryAddOrTransfer(pawn);
			activeDropPodInfo.openDelay = 180;
			activeDropPodInfo.leaveSlag = true;
			DropPodUtility.MakeDropPodAt(c, map, activeDropPodInfo);
		}
	}
}
