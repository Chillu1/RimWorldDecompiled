using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class FocusStrengthOffset_Curve : FocusStrengthOffset
{
	public SimpleCurve curve;

	protected abstract string ExplanationKey { get; }

	protected abstract float SourceValue(Thing parent);

	public override float GetOffset(Thing parent, Pawn user = null)
	{
		return Mathf.Round(curve.Evaluate(SourceValue(parent)) * 100f) / 100f;
	}

	public override string GetExplanation(Thing parent)
	{
		return ExplanationKey.Translate() + ": " + GetOffset(parent).ToStringWithSign("0%");
	}

	public override string GetExplanationAbstract(ThingDef def = null)
	{
		return ExplanationKey.Translate() + ": " + (curve[0].y.ToStringWithSign("0%") + " " + "RangeTo".Translate() + " " + curve[curve.PointsCount - 1].y.ToStringWithSign("0%"));
	}

	public override float MaxOffset(Thing parent = null)
	{
		float num = 0f;
		for (int i = 0; i < curve.PointsCount; i++)
		{
			float y = curve[i].y;
			if (Mathf.Abs(y) > Mathf.Abs(num))
			{
				num = y;
			}
		}
		return num;
	}
}
