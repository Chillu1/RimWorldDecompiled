using Verse;

namespace RimWorld
{
	public static class NegativeInteractionUtility
	{
		public const float AbrasiveSelectionChanceFactor = 2.3f;

		private static readonly SimpleCurve CompatibilityFactorCurve = new SimpleCurve
		{
			new CurvePoint(-2.5f, 4f),
			new CurvePoint(-1.5f, 3f),
			new CurvePoint(-0.5f, 2f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(1f, 0.75f),
			new CurvePoint(2f, 0.5f),
			new CurvePoint(3f, 0.4f)
		};

		private static readonly SimpleCurve OpinionFactorCurve = new SimpleCurve
		{
			new CurvePoint(-100f, 6f),
			new CurvePoint(-50f, 4f),
			new CurvePoint(-25f, 2f),
			new CurvePoint(0f, 1f),
			new CurvePoint(50f, 0.1f),
			new CurvePoint(100f, 0f)
		};

		public static float NegativeInteractionChanceFactor(Pawn initiator, Pawn recipient)
		{
			if (initiator.story.traits.HasTrait(TraitDefOf.Kind))
			{
				return 0f;
			}
			float num = 1f;
			num *= OpinionFactorCurve.Evaluate(initiator.relations.OpinionOf(recipient));
			num *= CompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
			if (initiator.story.traits.HasTrait(TraitDefOf.Abrasive))
			{
				num *= 2.3f;
			}
			return num;
		}
	}
}
