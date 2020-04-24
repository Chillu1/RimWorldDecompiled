using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StatPart_Age : StatPart
	{
		private SimpleCurve curve;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.ageTracker != null)
				{
					val *= AgeMultiplier(pawn);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.ageTracker != null)
				{
					return "StatsReport_AgeMultiplier".Translate(pawn.ageTracker.AgeBiologicalYears) + ": x" + AgeMultiplier(pawn).ToStringPercent();
				}
			}
			return null;
		}

		private float AgeMultiplier(Pawn pawn)
		{
			return curve.Evaluate((float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (curve == null)
			{
				yield return "curve is null.";
			}
		}
	}
}
