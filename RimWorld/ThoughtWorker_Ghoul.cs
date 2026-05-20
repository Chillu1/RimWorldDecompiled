using Verse;

namespace RimWorld;

public class ThoughtWorker_Ghoul : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!p.RaceProps.Humanlike)
		{
			return false;
		}
		if (!RelationsUtility.PawnsKnowEachOther(p, other))
		{
			return false;
		}
		return other.IsGhoul;
	}
}
