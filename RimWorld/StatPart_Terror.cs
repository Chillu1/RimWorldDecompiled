using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_Terror : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.Thing is Pawn thing)
		{
			val += TerrorUtility.SuppressionFallRateOverTerror.Evaluate(thing.GetStatValue(StatDefOf.Terror));
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn thing && !Mathf.Approximately(TerrorUtility.SuppressionFallRateOverTerror.Evaluate(thing.GetStatValue(StatDefOf.Terror)), 0f))
		{
			return "StatsReport_Terror".Translate() + (": " + TerrorUtility.SuppressionFallRateOverTerror.Evaluate(thing.GetStatValue(StatDefOf.Terror)).ToStringPercent());
		}
		return null;
	}
}
