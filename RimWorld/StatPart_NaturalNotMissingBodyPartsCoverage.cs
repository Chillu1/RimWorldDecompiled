using Verse;

namespace RimWorld;

public class StatPart_NaturalNotMissingBodyPartsCoverage : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetValue(req, out var value))
		{
			val *= value;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetValue(req, out var value))
		{
			return "StatsReport_MissingBodyParts".Translate() + ": x" + value.ToStringPercent();
		}
		return null;
	}

	private bool TryGetValue(StatRequest req, out float value)
	{
		return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => x.health.hediffSet.GetCoverageOfNotMissingNaturalParts(x.RaceProps.body.corePart), (ThingDef x) => 1f, out value);
	}
}
