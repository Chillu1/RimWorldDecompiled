using Verse;

namespace RimWorld;

public class ThoughtWorker_TaleDoublePawn : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
	{
		if (!other.RaceProps.Humanlike)
		{
			return false;
		}
		if (!RelationsUtility.PawnsKnowEachOther(p, other))
		{
			return false;
		}
		if (!(Find.TaleManager.GetLatestTale(def.taleDef, other) is Tale_DoublePawn tale_DoublePawn))
		{
			return false;
		}
		return (tale_DoublePawn.firstPawnData.pawn == p && tale_DoublePawn.secondPawnData.pawn == other) || (tale_DoublePawn.firstPawnData.pawn == other && tale_DoublePawn.secondPawnData.pawn == p);
	}
}
