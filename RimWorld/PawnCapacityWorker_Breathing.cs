using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Breathing : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.BreathingSource, float.MaxValue, default(FloatRange), impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.BreathingPathway, 1f, default(FloatRange), impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.BreathingSourceCage, 1f, default(FloatRange), impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.BreathingSource);
		}
	}
}
