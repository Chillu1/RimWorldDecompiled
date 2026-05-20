using Verse;

namespace RimWorld
{
	public abstract class ThoughtWorker_Precept : ThoughtWorker
	{
		protected abstract ThoughtState ShouldHaveThought(Pawn p);

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			Ideo ideo = p.Ideo;
			if (ideo == null)
			{
				return ThoughtState.Inactive;
			}
			Precept firstPreceptAllowingSituationalThought = ideo.GetFirstPreceptAllowingSituationalThought(def);
			if (firstPreceptAllowingSituationalThought != null && !firstPreceptAllowingSituationalThought.def.enabledForNPCFactions && !p.CountsAsNonNPCForPrecepts())
			{
				return ThoughtState.Inactive;
			}
			if (!ideo.cachedPossibleSituationalThoughts.Contains(def))
			{
				return ThoughtState.Inactive;
			}
			return ShouldHaveThought(p);
		}
	}
}
