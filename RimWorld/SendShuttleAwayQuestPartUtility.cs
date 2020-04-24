using Verse;

namespace RimWorld
{
	public static class SendShuttleAwayQuestPartUtility
	{
		public static void SendAway(Thing shuttle, bool dropEverything)
		{
			CompShuttle compShuttle = shuttle.TryGetComp<CompShuttle>();
			CompTransporter compTransporter = shuttle.TryGetComp<CompTransporter>();
			if (shuttle.Spawned)
			{
				if (dropEverything && compTransporter.LoadingInProgressOrReadyToLaunch)
				{
					compTransporter.CancelLoad();
				}
				if (!compTransporter.LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(compTransporter));
				}
				compShuttle.Send();
			}
			else if (shuttle.ParentHolder is Thing && ((Thing)shuttle.ParentHolder).def == ThingDefOf.ShuttleIncoming)
			{
				compShuttle.leaveASAP = true;
			}
		}
	}
}
