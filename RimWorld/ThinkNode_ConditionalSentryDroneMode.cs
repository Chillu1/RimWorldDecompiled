using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalSentryDroneMode : ThinkNode_Conditional
{
	public CompSentryDrone.SentryDroneMode mode;

	protected override bool Satisfied(Pawn pawn)
	{
		CompSentryDrone compSentryDrone = pawn.TryGetComp<CompSentryDrone>();
		if (compSentryDrone != null)
		{
			return compSentryDrone.Mode == mode;
		}
		return false;
	}
}
