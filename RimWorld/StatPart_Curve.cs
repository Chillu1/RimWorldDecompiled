using Verse;

namespace RimWorld
{
	public abstract class StatPart_Curve : StatPart
	{
		protected SimpleCurve curve;

		protected abstract bool AppliesTo(StatRequest req);

		protected abstract float CurveXGetter(StatRequest req);

		protected abstract string ExplanationLabel(StatRequest req);

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing && AppliesTo(req))
			{
				val *= curve.Evaluate(CurveXGetter(req));
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && AppliesTo(req))
			{
				return ExplanationLabel(req) + ": x" + curve.Evaluate(CurveXGetter(req)).ToStringPercent();
			}
			return null;
		}
	}
}
