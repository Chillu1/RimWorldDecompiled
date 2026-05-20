using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddShipJob_Wait : QuestNode_AddShipJob
{
	public SlateRef<int> ticks;

	public SlateRef<bool> leaveImmediatelyWhenSatisfied;

	public SlateRef<List<Thing>> sendAwayIfAllDespawned;

	protected override void AddJobVars(ShipJob shipJob, Slate slate)
	{
		if (shipJob is ShipJob_Wait shipJob_Wait)
		{
			shipJob_Wait.leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied.GetValue(slate);
			shipJob_Wait.sendAwayIfAllDespawned = sendAwayIfAllDespawned.GetValue(slate);
		}
		if (shipJob is ShipJob_WaitTime shipJob_WaitTime)
		{
			shipJob_WaitTime.duration = ticks.GetValue(slate);
		}
	}
}
