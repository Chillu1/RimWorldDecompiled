using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PrisonCellImpressiveness : ThoughtWorker_RoomImpressiveness
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.IsPrisoner)
			{
				return ThoughtState.Inactive;
			}
			ThoughtState result = base.CurrentStateInternal(p);
			if (result.Active && p.GetRoom().Role == RoomRoleDefOf.PrisonCell)
			{
				return result;
			}
			return ThoughtState.Inactive;
		}
	}
}
