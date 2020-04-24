using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Manipulation : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			float functionalPercentage = 0f;
			return PawnCapacityUtility.CalculateLimbEfficiency(diffSet, BodyPartTagDefOf.ManipulationLimbCore, BodyPartTagDefOf.ManipulationLimbSegment, BodyPartTagDefOf.ManipulationLimbDigit, 0.8f, out functionalPercentage, impactors) * CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.ManipulationLimbCore);
		}
	}
}
