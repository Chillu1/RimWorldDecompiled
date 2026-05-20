using Verse;

namespace RimWorld;

public class StatPart_Malnutrition : StatPart
{
	private SimpleCurve curve;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetMalnutritionFactor(req, out var _, out var factor))
		{
			val *= factor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetMalnutritionFactor(req, out var malnutritionSeverity, out var factor))
		{
			return "StatsReport_Malnutrition".Translate(malnutritionSeverity.ToStringPercent()) + ": x" + factor.ToStringPercent();
		}
		return null;
	}

	private bool TryGetMalnutritionFactor(StatRequest req, out float malnutritionSeverity, out float factor)
	{
		factor = 0f;
		malnutritionSeverity = 0f;
		if (!req.HasThing || !(req.Thing is Pawn pawn))
		{
			return false;
		}
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
		if (firstHediffOfDef == null)
		{
			return false;
		}
		malnutritionSeverity = firstHediffOfDef.Severity;
		factor = curve.Evaluate(malnutritionSeverity);
		return true;
	}
}
