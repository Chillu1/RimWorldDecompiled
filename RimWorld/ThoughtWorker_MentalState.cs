using Verse;

namespace RimWorld;

public class ThoughtWorker_MentalState : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		return p.MentalStateDef == def.mentalState;
	}
}
