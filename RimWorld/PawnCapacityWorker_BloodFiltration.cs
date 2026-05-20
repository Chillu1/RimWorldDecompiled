using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnCapacityWorker_BloodFiltration : PawnCapacityWorker
{
	public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
	{
		List<PawnCapacityUtility.CapacityImpactor> impactors2;
		if (diffSet.pawn.RaceProps.body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney))
		{
			BodyPartTagDef bloodFiltrationKidney = BodyPartTagDefOf.BloodFiltrationKidney;
			impactors2 = impactors;
			float num = PawnCapacityUtility.CalculateTagEfficiency(diffSet, bloodFiltrationKidney, float.MaxValue, default(FloatRange), impactors2);
			BodyPartTagDef bloodFiltrationLiver = BodyPartTagDefOf.BloodFiltrationLiver;
			impactors2 = impactors;
			return num * PawnCapacityUtility.CalculateTagEfficiency(diffSet, bloodFiltrationLiver, float.MaxValue, default(FloatRange), impactors2);
		}
		BodyPartTagDef bloodFiltrationSource = BodyPartTagDefOf.BloodFiltrationSource;
		impactors2 = impactors;
		return PawnCapacityUtility.CalculateTagEfficiency(diffSet, bloodFiltrationSource, float.MaxValue, default(FloatRange), impactors2);
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
