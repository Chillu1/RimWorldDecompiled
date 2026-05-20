using Verse;

namespace RimWorld
{
	public static class SendShuttleAwayQuestPartUtility
	{
		public static void SendAway(Thing shuttle, bool dropEverything)
		{
			CompShuttle compShuttle = shuttle.TryGetComp<CompShuttle>();
			if (compShuttle.shipParent == null)
			{
				compShuttle.shipParent = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, null, shuttle);
			}
			if (dropEverything)
			{
				compShuttle.shipParent.ForceJob(ShipJobDefOf.Unload);
				compShuttle.shipParent.AddJob(ShipJobDefOf.FlyAway);
			}
			else
			{
				compShuttle.shipParent.ForceJob(ShipJobDefOf.FlyAway);
			}
		}
	}
}
