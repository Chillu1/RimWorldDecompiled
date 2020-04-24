using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Man : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			if (!p.RaceProps.Humanlike)
			{
				return false;
			}
			if (!p.story.traits.HasTrait(TraitDefOf.DislikesMen))
			{
				return false;
			}
			if (!RelationsUtility.PawnsKnowEachOther(p, other))
			{
				return false;
			}
			if (other.def != p.def)
			{
				return false;
			}
			if (other.gender != Gender.Male)
			{
				return false;
			}
			return true;
		}
	}
}
