using Verse;

namespace RimWorld;

public class InteractionWorker_KindWords : InteractionWorker
{
	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		if (initiator.Inhumanized())
		{
			return 0f;
		}
		Trait trait = initiator.story.traits.GetTrait(TraitDefOf.Kind);
		if (trait != null && !trait.Suppressed)
		{
			return 0.01f;
		}
		return 0f;
	}
}
