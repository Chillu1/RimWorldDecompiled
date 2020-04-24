using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalAnyColonistTryingToExitMap : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			Map mapHeld = pawn.MapHeld;
			if (mapHeld == null)
			{
				return false;
			}
			foreach (Pawn item in mapHeld.mapPawns.FreeColonistsSpawned)
			{
				Job curJob = item.CurJob;
				if (curJob != null && curJob.exitMapOnArrival)
				{
					return true;
				}
			}
			return false;
		}
	}
}
