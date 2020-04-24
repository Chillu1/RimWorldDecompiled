using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_BloodFiltration : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			if (diffSet.pawn.RaceProps.body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney))
			{
				return PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.BloodFiltrationKidney, float.MaxValue, default(FloatRange), impactors) * PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.BloodFiltrationLiver, float.MaxValue, default(FloatRange), impactors);
			}
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, BodyPartTagDefOf.BloodFiltrationSource, float.MaxValue, default(FloatRange), impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			if (!body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney) || !body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationLiver))
			{
				return body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationSource);
			}
			return true;
		}
	}
}
