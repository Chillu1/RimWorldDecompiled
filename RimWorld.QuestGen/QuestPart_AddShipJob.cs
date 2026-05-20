using Verse;

namespace RimWorld.QuestGen
{
	public class QuestPart_AddShipJob : QuestPart
	{
		public string inSignal;

		public ShipJobDef shipJobDef;

		public TransportShip transportShip;

		public ShipJobStartMode shipJobStartMode;

		public ShipJob shipJob;

		public virtual ShipJob GetShipJob()
		{
			return shipJob ?? ShipJobMaker.MakeShipJob(shipJobDef);
		}

		public override bool QuestPartReserves(TransportShip ship)
		{
			return ship == transportShip;
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (signal.tag == inSignal)
			{
				switch (shipJobStartMode)
				{
				case ShipJobStartMode.Queue:
					transportShip.AddJob(GetShipJob());
					break;
				case ShipJobStartMode.Instant:
					transportShip.AddJob(GetShipJob());
					transportShip.TryGetNextJob();
					break;
				case ShipJobStartMode.Force:
					transportShip.ForceJob(GetShipJob());
					break;
				case ShipJobStartMode.Force_DelayCurrent:
					transportShip.ForceJob_DelayCurrent(GetShipJob());
					break;
				}
				shipJob = null;
			}
		}

		public override void Cleanup()
		{
			base.Cleanup();
			transportShip = null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref shipJobStartMode, "shipJobStartMode", ShipJobStartMode.Queue);
			Scribe_Deep.Look(ref shipJob, "shipJob");
			Scribe_Defs.Look(ref shipJobDef, "shipJobDef");
			Scribe_References.Look(ref transportShip, "transportShip");
		}
	}
}
