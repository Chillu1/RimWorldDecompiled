using Verse;

namespace RimWorld
{
	public class ThoughtWorker_AnnoyingVoice : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
			{
				return false;
			}
			if (!other.story.traits.HasTrait(TraitDefOf.AnnoyingVoice))
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
			{
				return false;
			}
			return true;
		}
	}
}
