using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NoThroneroom : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.royalty == null || p.MapHeld == null || !p.MapHeld.IsPlayerHome || p.royalty.HighestTitleWithThroneRoomRequirements() == null)
			{
				return false;
			}
			return p.ownership.AssignedThrone == null;
		}
	}
}
