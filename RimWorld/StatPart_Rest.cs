using System;
using Verse;

namespace RimWorld
{
	public class StatPart_Rest : StatPart
	{
		private float factorExhausted = 1f;

		private float factorVeryTired = 1f;

		private float factorTired = 1f;

		private float factorRested = 1f;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.needs.rest != null)
				{
					val *= RestMultiplier(pawn.needs.rest.CurCategory);
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && pawn.needs.rest != null)
				{
					return pawn.needs.rest.CurCategory.GetLabel() + ": x" + RestMultiplier(pawn.needs.rest.CurCategory).ToStringPercent();
				}
			}
			return null;
		}

		private float RestMultiplier(RestCategory fatigue)
		{
			switch (fatigue)
			{
			case RestCategory.Exhausted:
				return factorExhausted;
			case RestCategory.VeryTired:
				return factorVeryTired;
			case RestCategory.Tired:
				return factorTired;
			case RestCategory.Rested:
				return factorRested;
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
