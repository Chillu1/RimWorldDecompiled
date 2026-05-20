using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeComp_Inspired : SurgeryOutcomeComp_Factor
	{
		public InspirationDef inspirationDef;

		public override bool Affects(RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
		{
			if (surgeon.InspirationDef == inspirationDef)
			{
				return !patient.RaceProps.IsMechanoid;
			}
			return false;
		}

		public override void PreApply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			surgeon.mindState.inspirationHandler.EndInspiration(inspirationDef);
		}
	}
}
