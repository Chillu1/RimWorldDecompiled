using Verse;

namespace RimWorld;

public class ThoughtWorker_ImprisonedWithEntity : ThoughtWorker_EntityInRoomBase
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!p.IsPrisoner)
		{
			return ThoughtState.Inactive;
		}
		if (!p.guest.PrisonerIsSecure)
		{
			return ThoughtState.Inactive;
		}
		return base.CurrentStateInternal(p);
	}
}
