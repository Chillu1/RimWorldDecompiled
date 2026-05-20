using Verse;

namespace RimWorld.QuestGen
{
	public class QuestPart_AddShipJob_WaitTime : QuestPart_AddShipJob_Wait
	{
		public int duration;

		public override ShipJob GetShipJob()
		{
			ShipJob_WaitTime obj = (ShipJob_WaitTime)ShipJobMaker.MakeShipJob(shipJobDef);
			obj.duration = duration;
			obj.leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied;
			obj.showGizmos = showGizmos;
			obj.sendAwayIfAllDespawned = sendAwayIfAllDespawned;
			obj.sendAwayIfAnyDespawnedDownedOrDead = sendAwayIfAnyDespawnedDownedOrDead;
			return obj;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref duration, "duration", 0);
		}
	}
}
