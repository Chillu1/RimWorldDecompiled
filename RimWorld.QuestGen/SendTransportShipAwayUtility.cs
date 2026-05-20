namespace RimWorld.QuestGen
{
	public static class SendTransportShipAwayUtility
	{
		public static void SendTransportShipAway(TransportShip transportShip, bool unloadContents, TransportShipDropMode unsatisfiedDropMode = TransportShipDropMode.NonRequired)
		{
			if (transportShip == null || transportShip.Disposed)
			{
				return;
			}
			if (!transportShip.started)
			{
				transportShip.Dispose();
			}
			else if (transportShip.ShipExistsAndIsSpawned && !transportShip.LeavingSoonAutomatically)
			{
				ShipJob_FlyAway shipJob_FlyAway = (ShipJob_FlyAway)ShipJobMaker.MakeShipJob(ShipJobDefOf.FlyAway);
				shipJob_FlyAway.dropMode = unsatisfiedDropMode;
				if (unloadContents)
				{
					transportShip.ForceJob(ShipJobDefOf.Unload);
					transportShip.AddJob(shipJob_FlyAway);
				}
				else
				{
					transportShip.ForceJob(shipJob_FlyAway);
				}
			}
		}
	}
}
