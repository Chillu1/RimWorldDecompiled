using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ExpectationsSlave : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.IsSlave)
			{
				return ThoughtState.Inactive;
			}
			ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(p);
			if (expectationDef == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(GetThoughtStageForExpectation(expectationDef));
		}

		private int GetThoughtStageForExpectation(ExpectationDef expectation)
		{
			if (expectation == ExpectationDefOf.ExtremelyLow)
			{
				return 0;
			}
			if (expectation == ExpectationDefOf.VeryLow)
			{
				return 1;
			}
			if (expectation == ExpectationDefOf.Low)
			{
				return 2;
			}
			return 3;
		}
	}
}
