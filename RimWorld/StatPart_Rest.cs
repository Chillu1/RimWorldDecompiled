using System;
using Verse;

namespace RimWorld;

public class StatPart_Rest : StatPart
{
	private float factorExhausted = 1f;

	private float factorVeryTired = 1f;

	private float factorTired = 1f;

	private float factorRested = 1f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn pawn && pawn.needs.rest != null)
		{
			val *= RestMultiplier(pawn.needs.rest.CurCategory);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn pawn && pawn.needs.rest != null)
		{
			return pawn.needs.rest.CurCategory.GetLabel() + ": x" + RestMultiplier(pawn.needs.rest.CurCategory).ToStringPercent();
		}
		return null;
	}

	private float RestMultiplier(RestCategory fatigue)
	{
		return fatigue switch
		{
			RestCategory.Exhausted => factorExhausted, 
			RestCategory.VeryTired => factorVeryTired, 
			RestCategory.Tired => factorTired, 
			RestCategory.Rested => factorRested, 
			_ => throw new InvalidOperationException(), 
		};
	}
}
