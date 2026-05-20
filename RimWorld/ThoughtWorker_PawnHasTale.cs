using Verse;

namespace RimWorld;

public class ThoughtWorker_PawnHasTale : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!p.RaceProps.Humanlike)
		{
			return false;
		}
		if (Find.TaleManager.GetLatestTale(def.taleDef, p) == null)
		{
			return false;
		}
		return true;
	}
}
