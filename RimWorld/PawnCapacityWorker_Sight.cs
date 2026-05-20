using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnCapacityWorker_Sight : PawnCapacityWorker
{
	public static float PartEfficiencySpecialWeight = 0.75f;

	public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
	{
		BodyPartTagDef sightSource = BodyPartTagDefOf.SightSource;
		float partEfficiencySpecialWeight = PartEfficiencySpecialWeight;
		return PawnCapacityUtility.CalculateTagEfficiency(diffSet, sightSource, float.MaxValue, default(FloatRange), impactors, partEfficiencySpecialWeight);
	}

	public override bool CanHaveCapacity(BodyDef body)
	{
		return body.HasPartWithTag(BodyPartTagDefOf.SightSource);
	}
}
