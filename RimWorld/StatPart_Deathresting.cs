using Verse;

namespace RimWorld;

public class StatPart_Deathresting : StatPart
{
	public float factor;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn { Deathresting: not false })
		{
			val *= factor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn { Deathresting: not false })
		{
			return "Deathresting".Translate().CapitalizeFirst() + ": x" + factor.ToStringPercent();
		}
		return null;
	}
}
