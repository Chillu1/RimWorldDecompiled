using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PrisonBarracksImpressiveness : ThoughtWorker_RoomImpressiveness
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.IsPrisoner)
			{
				return ThoughtState.Inactive;
			}
			ThoughtState result = base.CurrentStateInternal(p);
			if (result.Active && p.GetRoom().Role == RoomRoleDefOf.PrisonBarracks)
			{
				return result;
			}
			return ThoughtState.Inactive;
		}
	}
}
