using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Scarification : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!p.IsColonist)
			{
				return false;
			}
			int hediffCount = p.health.hediffSet.GetHediffCount(HediffDefOf.Scarification);
			int requiredScars = p.ideo.Ideo.RequiredScars;
			if (hediffCount >= requiredScars)
			{
				return ProcessedState(0, p);
			}
			if (hediffCount == 0)
			{
				return ProcessedState(1, p);
			}
			if (hediffCount < requiredScars)
			{
				return ProcessedState(2, p);
			}
			return false;
		}

		private ThoughtState ProcessedState(int index, Pawn p)
		{
			if (def.stages[index].baseMoodEffect < 0f && def.minExpectationForNegativeThought != null && p.MapHeld != null && ExpectationsUtility.CurrentExpectationFor(p.MapHeld).order < def.minExpectationForNegativeThought.order)
			{
				return false;
			}
			return ThoughtState.ActiveAtStage(index);
		}
	}
}
