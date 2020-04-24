using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetJoyInGatheringArea : JobGiver_GetJoy
	{
		protected override Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
		{
			if (pawn.mindState.duty == null)
			{
				return null;
			}
			if (pawn.needs.joy == null)
			{
				return null;
			}
			if (pawn.needs.joy.CurLevelPercentage > 0.92f)
			{
				return null;
			}
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			return def.Worker.TryGiveJobInGatheringArea(pawn, cell);
		}
	}
}
