using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeComp_XenogermComplexity : SurgeryOutcomeComp_Curve
	{
		protected override float XGetter(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			Xenogerm xenogerm = bill.xenogerm;
			if (xenogerm?.GeneSet == null)
			{
				return 0f;
			}
			return xenogerm.GeneSet.ComplexityTotal;
		}
	}
}
