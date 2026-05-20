using System.Collections.Generic;

namespace Verse;

public class HediffCompProperties_ReplaceHediff : HediffCompProperties
{
	public class TriggeredHediff
	{
		public HediffDef hediff;

		public IntRange countRange = new IntRange(1, 1);

		public List<BodyPartDef> partsToAffect;

		public FloatRange severityRange = FloatRange.Zero;

		public void ApplyTo(Pawn pawn, List<Hediff> outAddedHediffs = null)
		{
			List<Hediff> list = new List<Hediff>();
			HediffGiverUtility.TryApply(pawn, hediff, partsToAffect, canAffectAnyLivePart: false, countRange.RandomInRange, list, useCoverage: false);
			if (severityRange != FloatRange.Zero)
			{
				foreach (Hediff item in list)
				{
					item.Severity = severityRange.RandomInRange;
				}
			}
			outAddedHediffs?.AddRange(list);
		}
	}

	public float severity = 1f;

	public bool manuallyTriggered;

	[MustTranslate]
	public string message;

	public MessageTypeDef messageDef;

	[MustTranslate]
	public string letterLabel;

	[MustTranslate]
	public string letterDesc;

	public LetterDef letterDef;

	public List<TriggeredHediff> hediffs = new List<TriggeredHediff>();

	public HediffCompProperties_ReplaceHediff()
	{
		compClass = typeof(HediffComp_ReplaceHediff);
	}
}
