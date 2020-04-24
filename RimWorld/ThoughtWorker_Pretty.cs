using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Pretty : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
			{
				return false;
			}
			if (RelationsUtility.IsDisfigured(other))
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return false;
			}
			float statValue = other.GetStatValue(StatDefOf.PawnBeauty);
			if (statValue >= 2f)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (statValue >= 1f)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			return false;
		}
	}
}
