using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Tale : ThoughtWorker
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
			if (Find.TaleManager.GetLatestTale(def.taleDef, other) == null)
			{
				return false;
			}
			return true;
		}
	}
}
