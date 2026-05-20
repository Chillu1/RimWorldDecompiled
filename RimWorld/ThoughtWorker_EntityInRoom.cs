using Verse;

namespace RimWorld;

public class ThoughtWorker_EntityInRoom : ThoughtWorker_EntityInRoomBase
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.IsPrisoner)
		{
			return ThoughtState.Inactive;
		}
		if (!p.IsColonist)
		{
			return ThoughtState.Inactive;
		}
		return base.CurrentStateInternal(p);
	}
}
