namespace RimWorld.QuestGen
{
	public class QuestPart_AddShipJob_WaitForever : QuestPart_AddShipJob_Wait
	{
		public override ShipJob GetShipJob()
		{
			ShipJob_WaitForever obj = (ShipJob_WaitForever)ShipJobMaker.MakeShipJob(shipJobDef);
			obj.leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied;
			obj.showGizmos = showGizmos;
			obj.sendAwayIfAllDespawned = sendAwayIfAllDespawned;
			obj.sendAwayIfAnyDespawnedDownedOrDead = sendAwayIfAnyDespawnedDownedOrDead;
			return obj;
		}
	}
}
