using Verse;

namespace RimWorld
{
	public class ThoughtWorker_TranshumanistAppreciation : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			if (!p.RaceProps.Humanlike)
			{
				return false;
			}
			if (!p.story.traits.HasTrait(TraitDefOf.Transhumanist))
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
			int num = other.health.hediffSet.CountAddedAndImplantedParts();
			if (num > 0)
			{
				return ThoughtState.ActiveAtStage(num - 1);
			}
			return false;
		}
	}
}
