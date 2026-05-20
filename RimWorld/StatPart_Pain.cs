using Verse;

namespace RimWorld;

public class StatPart_Pain : StatPart
{
	private float factor = 1f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.Thing is Pawn pawn)
		{
			val *= PainFactor(pawn);
		}
	}

	public float PainFactor(Pawn pawn)
	{
		return 1f + pawn.health.hediffSet.PainTotal * factor;
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn pawn)
		{
			return "StatsReport_Pain".Translate() + (": " + PainFactor(pawn).ToStringPercent("F0"));
		}
		return null;
	}
}
