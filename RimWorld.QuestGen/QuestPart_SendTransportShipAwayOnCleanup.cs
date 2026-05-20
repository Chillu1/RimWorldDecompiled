using Verse;

namespace RimWorld.QuestGen
{
	public class QuestPart_SendTransportShipAwayOnCleanup : QuestPart
	{
		public TransportShip transportShip;

		public TransportShipDropMode unsatisfiedDropMode;

		public bool unloadContents;

		public override bool QuestPartReserves(TransportShip ship)
		{
			return ship == transportShip;
		}

		public override void Cleanup()
		{
			SendTransportShipAwayUtility.SendTransportShipAway(transportShip, unloadContents, unsatisfiedDropMode);
			transportShip = null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref unloadContents, "unloadContents", defaultValue: false);
			Scribe_Values.Look(ref unsatisfiedDropMode, "unsatisfiedDropMode", TransportShipDropMode.None);
			Scribe_References.Look(ref transportShip, "transportShip");
		}
	}
}
