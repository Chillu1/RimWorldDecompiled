using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Disfigured : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike || other.Dead)
			{
				return false;
			}
			if (!RelationsUtility.PawnsKnowEachOther(pawn, other))
			{
				return false;
			}
			if (!RelationsUtility.IsDisfigured(other))
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return false;
			}
			return true;
		}
	}
}
