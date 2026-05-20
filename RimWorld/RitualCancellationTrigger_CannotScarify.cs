using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RitualCancellationTrigger_CannotScarify : RitualCancellationTrigger
{
	[NoTranslate]
	public string roleId;

	public override IEnumerable<Trigger> CancellationTriggers(RitualRoleAssignments assignments)
	{
		yield return new Trigger_Custom(delegate(TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.Tick && GenTicks.TicksGame % 60 == 0)
			{
				Pawn pawn = assignments.FirstAssignedPawn(roleId);
				if (pawn == null)
				{
					return true;
				}
				if (!JobDriver_Scarify.AvailableOnNow(pawn))
				{
					return true;
				}
			}
			return false;
		});
	}
}
