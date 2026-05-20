using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_AddShipJob_WaitSendable : QuestPart_AddShipJob_Wait
{
	public MapParent destination;

	public bool targetPlayerSettlement;

	public override ShipJob GetShipJob()
	{
		ShipJob_WaitSendable obj = (ShipJob_WaitSendable)ShipJobMaker.MakeShipJob(shipJobDef);
		obj.destination = destination;
		obj.leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied;
		obj.showGizmos = showGizmos;
		obj.sendAwayIfAllDespawned = sendAwayIfAllDespawned;
		obj.sendAwayIfAnyDespawnedDownedOrDead = sendAwayIfAnyDespawnedDownedOrDead;
		obj.targetPlayerSettlement = targetPlayerSettlement;
		return obj;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref destination, "destination");
		Scribe_Values.Look(ref targetPlayerSettlement, "targetPlayerSettlement", defaultValue: false);
	}
}
