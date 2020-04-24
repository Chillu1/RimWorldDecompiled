using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Talking : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.TalkingSource, float.MaxValue, default(FloatRange), impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.TalkingPathway, 1f, default(FloatRange), impactors) * CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.TalkingSource);
		}
	}
}
