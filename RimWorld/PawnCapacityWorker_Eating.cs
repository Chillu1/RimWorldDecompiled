using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnCapacityWorker_Eating : PawnCapacityWorker
{
	public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
	{
		BodyPartTagDef eatingSource = BodyPartTagDefOf.EatingSource;
		List<PawnCapacityUtility.CapacityImpactor> impactors2 = impactors;
		float num = PawnCapacityUtility.CalculateTagEfficiency(diffSet, eatingSource, float.MaxValue, default(FloatRange), impactors2);
		BodyPartTagDef eatingPathway = BodyPartTagDefOf.EatingPathway;
		impactors2 = impactors;
		return num * PawnCapacityUtility.CalculateTagEfficiency(diffSet, eatingPathway, 1f, default(FloatRange), impactors2) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.Tongue, float.MaxValue, new FloatRange(0.5f, 1f), impactors) * CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
	}

	public override bool CanHaveCapacity(BodyDef body)
	{
		return body.HasPartWithTag(BodyPartTagDefOf.EatingSource);
	}
}
