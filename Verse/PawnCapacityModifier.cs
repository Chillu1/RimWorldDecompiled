using RimWorld;

namespace Verse
{
	public class PawnCapacityModifier
	{
		public PawnCapacityDef capacity;

		public float offset;

		public float setMax = 999f;

		public float postFactor = 1f;

		public SimpleCurve setMaxCurveOverride;

		public StatDef setMaxCurveEvaluateStat;

		public bool SetMaxDefined
		{
			get
			{
				if (setMax == 999f)
				{
					if (setMaxCurveOverride != null)
					{
						return setMaxCurveEvaluateStat != null;
					}
					return false;
				}
				return true;
			}
		}

		public float EvaluateSetMax(Pawn pawn)
		{
			if (setMaxCurveOverride == null || setMaxCurveEvaluateStat == null)
			{
				return setMax;
			}
			return setMaxCurveOverride.Evaluate(pawn.GetStatValue(setMaxCurveEvaluateStat));
		}
	}
}
