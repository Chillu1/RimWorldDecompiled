using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public abstract class QuestPart_AddShipJob_Wait : QuestPart_AddShipJob
	{
		public bool leaveImmediatelyWhenSatisfied;

		public bool showGizmos = true;

		public List<Thing> sendAwayIfAllDespawned;

		public List<Thing> sendAwayIfAnyDespawnedDownedOrDead;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref leaveImmediatelyWhenSatisfied, "leaveImmediatelyWhenSatisfied", defaultValue: false);
			Scribe_Values.Look(ref showGizmos, "showGizmos", defaultValue: false);
			Scribe_Collections.Look(ref sendAwayIfAllDespawned, "sendAwayIfAllDespawned", LookMode.Reference);
			Scribe_Collections.Look(ref sendAwayIfAnyDespawnedDownedOrDead, "sendAwayIfAnyDespawnedDownedOrDead", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				sendAwayIfAllDespawned?.RemoveAll((Thing x) => x == null);
				sendAwayIfAnyDespawnedDownedOrDead?.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
