using Verse;

namespace RimWorld;

public class StatPart_ShamblerCorpse : StatPart
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
			return "StatsReport_MultiplierFor".Translate(HediffDefOf.ShamblerCorpse.label) + (": x" + multiplier.ToStringPercent());
		}
		return null;
	}

	private bool ActiveFor(Thing t)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (t == null)
		{
			return false;
		}
		if (!(t is Corpse corpse))
		{
			return false;
		}
		return corpse.InnerPawn.health.hediffSet.HasHediff(HediffDefOf.ShamblerCorpse);
	}
}
