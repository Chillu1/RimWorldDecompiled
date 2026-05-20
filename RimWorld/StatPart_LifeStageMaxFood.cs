using Verse;

namespace RimWorld;

public class StatPart_LifeStageMaxFood : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn pawn)
		{
			val *= pawn.ageTracker.CurLifeStage.foodMaxFactor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn pawn)
		{
			return "LifeStageMaxFood".Translate() + ": x" + pawn.ageTracker.CurLifeStage.foodMaxFactor.ToStringPercent();
		}
		return null;
	}
}
