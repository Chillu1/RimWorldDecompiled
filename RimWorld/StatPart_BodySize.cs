using Verse;

namespace RimWorld;

public class StatPart_BodySize : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetBodySize(req, out var bodySize))
		{
			val *= bodySize;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetBodySize(req, out var bodySize))
		{
			return "StatsReport_BodySize".Translate(bodySize.ToString("F2")) + ": x" + bodySize.ToStringPercent();
		}
		return null;
	}

	private bool TryGetBodySize(StatRequest req, out float bodySize)
	{
		return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => x.BodySize, (ThingDef x) => x.race.baseBodySize, out bodySize);
	}
}
