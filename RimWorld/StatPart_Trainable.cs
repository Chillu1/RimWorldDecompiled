using Verse;

namespace RimWorld;

public class StatPart_Trainable : StatPart
{
	public TrainableDef trainableDef;

	public float factor = 1f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (Applies(req))
		{
			val *= factor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (Applies(req))
		{
			return "StatsReport_Trainable".Translate(trainableDef.LabelCap) + (": " + factor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
		}
		return null;
	}

	private bool Applies(StatRequest req)
	{
		if (!(req.Thing is Pawn { IsAnimal: not false } pawn))
		{
			return false;
		}
		return pawn.training?.HasLearned(TrainableDefOf.Dig) ?? false;
	}

	public override bool ForceShow(StatRequest req)
	{
		return Applies(req);
	}
}
