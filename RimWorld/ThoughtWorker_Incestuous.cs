using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Incestuous : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike)
			{
				return false;
			}
			if (!RelationsUtility.PawnsKnowEachOther(pawn, other))
			{
				return false;
			}
			if (LovePartnerRelationUtility.IncestOpinionOffsetFor(other, pawn) == 0f)
			{
				return false;
			}
			return true;
		}
	}
}
