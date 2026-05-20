using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeComp_PatientAge : SurgeryOutcomeComp_Curve
	{
		protected override float XGetter(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			return patient.ageTracker.AgeBiologicalYearsFloat;
		}
	}
}
