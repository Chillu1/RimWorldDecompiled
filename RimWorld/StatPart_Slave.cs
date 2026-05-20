using Verse;

namespace RimWorld;

public class StatPart_Slave : StatPart
{
	private float factor = 0.75f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (ActiveFor(req.Thing))
		{
			val *= factor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && ActiveFor(req.Thing))
		{
			return "StatsReport_Slave".Translate() + (": x" + factor.ToStringPercent());
		}
		return null;
	}

	private bool ActiveFor(Thing t)
	{
		if (t is Pawn pawn)
		{
			return pawn.IsSlave;
		}
		return false;
	}
}
