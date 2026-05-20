using Verse;

namespace RimWorld;

public class StatPart_ShamblerCrawling : StatPart
{
	public float factor;

	private bool ActiveFor(Thing t)
	{
		if (!t.Spawned)
		{
			return false;
		}
		if (!(t is Pawn { IsShambler: not false } pawn))
		{
			return false;
		}
		return pawn.Crawling;
	}

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && ActiveFor(req.Thing))
		{
			val *= factor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && ActiveFor(req.Thing))
		{
			return "StatsReport_ShamblerCrawling".Translate() + ": x" + factor.ToStringPercent();
		}
		return null;
	}
}
