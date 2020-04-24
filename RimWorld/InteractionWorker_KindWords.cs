using Verse;

namespace RimWorld
{
	public class InteractionWorker_KindWords : InteractionWorker
	{
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (initiator.story.traits.HasTrait(TraitDefOf.Kind))
			{
				return 0.01f;
			}
			return 0f;
		}
	}
}
