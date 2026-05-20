using Verse;

namespace RimWorld;

public class StatPart_GrowthVatSpeedFactor : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetFactor(req, out var value))
		{
			val *= value;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetFactor(req, out var value))
		{
			return "StatsReport_GrowthVatSpeedFactor".Translate() + ": " + value.ToStringPercent();
		}
		return null;
	}

	private bool TryGetFactor(StatRequest req, out float value)
	{
		if (!req.HasThing || !(req.Thing is Pawn { Ideo: not null } pawn))
		{
			value = 0f;
			return false;
		}
		value = 1f;
		foreach (Precept item in pawn.Ideo.PreceptsListForReading)
		{
			value *= item.def.growthVatSpeedFactor;
		}
		return true;
	}
}
