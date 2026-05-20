using RimWorld;

namespace Verse.AI.Group;

public class Trigger_UrgentlyHungry : Trigger
{
	public override bool ActivateOn(Lord lord, TriggerSignal signal)
	{
		if (signal.type == TriggerSignalType.Tick)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn_NeedsTracker needs = lord.ownedPawns[i].needs;
				if (needs != null && (int?)needs.food?.CurCategory >= (int?)2)
				{
					return true;
				}
			}
		}
		return false;
	}
}
