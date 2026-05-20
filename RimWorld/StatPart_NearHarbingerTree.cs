using Verse;

namespace RimWorld;

public class StatPart_NearHarbingerTree : StatPart
{
	private float multiplier = 1f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (ActiveFor(req.Thing))
		{
			val *= multiplier;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && ActiveFor(req.Thing))
		{
			return "StatsReport_MultiplierFor".Translate(ThingDefOf.Plant_TreeHarbinger.label) + (": x" + multiplier.ToStringPercent());
		}
		return null;
	}

	private bool ActiveFor(Thing t)
	{
		if (t == null)
		{
			return false;
		}
		return t.TryGetComp<CompHarbingerTreeConsumable>()?.BeingConsumed ?? false;
	}
}
