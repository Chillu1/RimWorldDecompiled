using Verse;

namespace RimWorld;

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
		if (!RelationsUtility.IsDisfigured(other, pawn))
		{
			return false;
		}
		if (PawnUtility.IsBiologicallyBlind(pawn))
		{
			return false;
		}
		if (!pawn.story.CaresAboutOthersAppearance)
		{
			return false;
		}
		if (pawn.Ideo != null && pawn.Ideo.IdeoApprovesOfBlindness() && !RelationsUtility.IsDisfigured(other, pawn, ignoreSightSources: true) && (PawnUtility.IsBiologicallyBlind(other) || ThoughtWorker_Precept_HalfBlind.IsHalfBlind(other)))
		{
			return false;
		}
		return true;
	}
}
