using Verse;

namespace RimWorld;

public class StatPart_OverseerStatOffset : StatPart
{
	private StatDef stat;

	[MustTranslate]
	private string label;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetOffset(req, out var offset))
		{
			val += offset;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetOffset(req, out var offset) && offset != 0f)
		{
			return label + ": +" + offset.ToStringPercent();
		}
		return null;
	}

	private bool TryGetOffset(StatRequest req, out float offset)
	{
		if (ModsConfig.BiotechActive && req.HasThing && req.Thing is Pawn pawn)
		{
			Pawn overseer = pawn.GetOverseer();
			if (overseer != null)
			{
				offset = overseer.GetStatValue(stat);
				return true;
			}
		}
		offset = 0f;
		return false;
	}
}
