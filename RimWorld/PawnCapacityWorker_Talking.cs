using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnCapacityWorker_Talking : PawnCapacityWorker
{
	public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
	{
		BodyPartTagDef talkingSource = BodyPartTagDefOf.TalkingSource;
		List<PawnCapacityUtility.CapacityImpactor> impactors2 = impactors;
		float num = PawnCapacityUtility.CalculateTagEfficiency(diffSet, talkingSource, float.MaxValue, default(FloatRange), impactors2);
		BodyPartTagDef talkingPathway = BodyPartTagDefOf.TalkingPathway;
		impactors2 = impactors;
		float num2 = num * PawnCapacityUtility.CalculateTagEfficiency(diffSet, talkingPathway, 1f, default(FloatRange), impactors2);
		BodyPartTagDef tongue = BodyPartTagDefOf.Tongue;
		impactors2 = impactors;
		return num2 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, tongue, 1f, default(FloatRange), impactors2) * CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
	}

	public override bool CanHaveCapacity(BodyDef body)
	{
		return body.HasPartWithTag(BodyPartTagDefOf.TalkingSource);
	}
}
