using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class StatPart_NotCarefullySlaughtered : StatPart
{
	private float factor;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (HasWounds(req))
		{
			val *= factor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (HasWounds(req))
		{
			return "StatsReport_HasHediffExplanation".Translate() + ": x" + factor.ToStringPercent();
		}
		return null;
	}

	private bool HasWounds(StatRequest req)
	{
		if (!req.HasThing || !(req.Thing is Pawn pawn))
		{
			return false;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def != HediffDefOf.ExecutionCut && typeof(Hediff_Injury).IsAssignableFrom(hediffs[i].def.hediffClass) && !hediffs[i].IsPermanent())
			{
				return true;
			}
		}
		return false;
	}
}
