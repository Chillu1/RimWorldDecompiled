using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_HighLife : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
	{
		private const float DaysSatisfied = 0.75f;

		private const float DaysNoBonus = 1f;

		private const float DaysMissing = 2f;

		private const float DaysMissing_Major = 11f;

		public static readonly SimpleCurve MoodOffsetFromDaysSinceLastDrugCurve = new SimpleCurve
		{
			new CurvePoint(0.75f, 3f),
			new CurvePoint(1f, 0f),
			new CurvePoint(2f, -1f),
			new CurvePoint(11f, -10f)
		};

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!ThoughtUtility.ThoughtNullified(p, def))
			{
				float num = (float)(Find.TickManager.TicksGame - p.mindState.lastTakeRecreationalDrugTick) / 60000f;
				if (num > 1f && def.minExpectationForNegativeThought != null && p.MapHeld != null && ExpectationsUtility.CurrentExpectationFor(p.MapHeld).order < def.minExpectationForNegativeThought.order)
				{
					return false;
				}
				if (num < 1f)
				{
					return ThoughtState.ActiveAtStage(0);
				}
				if (num < 2f)
				{
					return ThoughtState.ActiveAtStage(1);
				}
				if (num < 11f)
				{
					return ThoughtState.ActiveAtStage(2);
				}
				return ThoughtState.ActiveAtStage(3);
			}
			return false;
		}

		public IEnumerable<NamedArgument> GetDescriptionArgs()
		{
			yield return 0.75f.ToString("F2").Named("DAYSSATISIFED");
		}
	}
}
