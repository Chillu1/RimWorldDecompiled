using Verse;

namespace RimWorld;

public class StatPart_IsFlesh : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetIsFleshFactor(req, out var bodySize))
		{
			val *= bodySize;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetIsFleshFactor(req, out var bodySize) && bodySize != 1f)
		{
			return "StatsReport_NotFlesh".Translate() + ": x" + bodySize.ToStringPercent();
		}
		return null;
	}

	private bool TryGetIsFleshFactor(StatRequest req, out float bodySize)
	{
		return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, (Pawn x) => (!x.RaceProps.IsFlesh) ? 0f : 1f, (ThingDef x) => (!x.race.IsFlesh) ? 0f : 1f, out bodySize);
	}
}
