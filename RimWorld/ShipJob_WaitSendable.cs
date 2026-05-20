using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class ShipJob_WaitSendable : ShipJob_Wait
{
	public MapParent destination;

	private bool sentMessage;

	protected override bool ShouldEnd => false;

	public override bool HasDestination => destination != null;

	public override IEnumerable<Gizmo> GetJobGizmos()
	{
		return Enumerable.Empty<Gizmo>();
	}

	protected override void SendAway()
	{
		if (targetPlayerSettlement && (destination == null || !destination.HasMap))
		{
			MapParent mapParent = null;
			foreach (Settlement settlement in Find.World.worldObjects.Settlements)
			{
				if (settlement.HasMap && settlement.Faction == Faction.OfPlayer && (mapParent == null || settlement.Map.mapPawns.ColonistCount > mapParent.Map.mapPawns.ColonistCount))
				{
					mapParent = settlement;
				}
			}
			if (mapParent == null)
			{
				mapParent = Find.AnyPlayerHomeMap.Parent;
			}
			if (mapParent == null)
			{
				if (!sentMessage)
				{
					Messages.Message("ShipNoSettlementToReturnTo".Translate(), transportShip.shipThing, MessageTypeDefOf.CautionInput);
					sentMessage = true;
				}
				return;
			}
			destination = mapParent;
		}
		ShipJob_FlyAway shipJob_FlyAway = (ShipJob_FlyAway)ShipJobMaker.MakeShipJob(ShipJobDefOf.FlyAway);
		shipJob_FlyAway.destinationTile = destination.Tile;
		shipJob_FlyAway.arrivalAction = new TransportersArrivalAction_TransportShip(destination, transportShip);
		shipJob_FlyAway.dropMode = TransportShipDropMode.None;
		transportShip.SetNextJob(shipJob_FlyAway);
		transportShip.TryGetNextJob();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref destination, "destination");
		Scribe_Values.Look(ref sentMessage, "sentMessage", defaultValue: false);
	}
}
