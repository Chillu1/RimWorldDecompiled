using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnCapacityWorker_Breathing : PawnCapacityWorker
{
	public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
	{
		BodyPartTagDef breathingSource = BodyPartTagDefOf.BreathingSource;
		List<PawnCapacityUtility.CapacityImpactor> impactors2 = impactors;
		float num = PawnCapacityUtility.CalculateTagEfficiency(diffSet, breathingSource, float.MaxValue, default(FloatRange), impactors2);
		BodyPartTagDef breathingPathway = BodyPartTagDefOf.BreathingPathway;
		impactors2 = impactors;
		float num2 = num * PawnCapacityUtility.CalculateTagEfficiency(diffSet, breathingPathway, 1f, default(FloatRange), impactors2);
		BodyPartTagDef breathingSourceCage = BodyPartTagDefOf.BreathingSourceCage;
		impactors2 = impactors;
		return num2 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, breathingSourceCage, 1f, default(FloatRange), impactors2);
	}

	public override bool CanHaveCapacity(BodyDef body)
	{
		return body.HasPartWithTag(BodyPartTagDefOf.BreathingSource);
	}
}
