using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StatPart_AgeOffset : StatPart
{
	private SimpleCurve curve;

	private bool useBiologicalYears;

	private bool humanlikeOnly;

	private bool ActiveFor(Pawn pawn)
	{
		if (pawn.ageTracker == null)
		{
			return false;
		}
		if (humanlikeOnly && !pawn.RaceProps.Humanlike)
		{
			return false;
		}
		return true;
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn pawn && ActiveFor(pawn))
		{
			val += AgeOffset(pawn);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn pawn && ActiveFor(pawn))
		{
			return "StatsReport_AgeOffset".Translate(pawn.ageTracker.AgeBiologicalYears) + ": " + AgeOffset(pawn).ToStringByStyle(parentStat.toStringStyle, parentStat.toStringNumberSense);
		}
		return null;
	}

	private float AgeOffset(Pawn pawn)
	{
		if (!useBiologicalYears)
		{
			return curve.Evaluate((float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy);
		}
		return curve.Evaluate(pawn.ageTracker.AgeBiologicalYears);
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (curve == null)
		{
			yield return "curve is null.";
		}
	}
}
