using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Expectations : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(p);
			if (expectationDef == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(expectationDef.thoughtStage);
		}
	}
}
