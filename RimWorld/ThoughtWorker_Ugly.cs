using Verse;

namespace RimWorld;

public class ThoughtWorker_Ugly : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
	{
		if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
		{
			return false;
		}
		if (PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn))
		{
			return false;
		}
		if (!pawn.story.CaresAboutOthersAppearance)
		{
			return false;
		}
		float statValue = other.GetStatValue(StatDefOf.PawnBeauty);
		if (statValue <= -2f)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		if (statValue <= -1f)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		return false;
	}
}
