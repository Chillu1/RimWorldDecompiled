using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StatPart_Mood : StatPart
	{
		private SimpleCurve factorFromMoodCurve;

		public override IEnumerable<string> ConfigErrors()
		{
			if (factorFromMoodCurve == null)
			{
				yield return "curve is null.";
			}
		}

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && ActiveFor(pawn))
				{
					val *= FactorFromMood(pawn);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && ActiveFor(pawn))
				{
					return "StatsReport_MoodMultiplier".Translate(pawn.needs.mood.CurLevel.ToStringPercent()) + ": x" + FactorFromMood(pawn).ToStringPercent();
				}
			}
			return null;
		}

		private bool ActiveFor(Pawn pawn)
		{
			return pawn.needs.mood != null;
		}

		private float FactorFromMood(Pawn pawn)
		{
			return factorFromMoodCurve.Evaluate(pawn.needs.mood.CurLevel);
		}
	}
}
