using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeComp_SurgeonSuccessChance : SurgeryOutcomeComp
	{
		public override bool Affects(RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
		{
			return !patient.RaceProps.IsMechanoid;
		}

		public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
		{
			quality *= surgeon.GetStatValue(StatDefOf.MedicalSurgerySuccessChance);
		}
	}
}
