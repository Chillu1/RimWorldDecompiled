using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Hearing : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.HearingSource, float.MaxValue, default(FloatRange), impactors, 0.75f);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.HearingSource);
		}
	}
}
