using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class SurgeryOutcomeComp_Curve : SurgeryOutcomeComp
	{
		public SimpleCurve curve;

		protected abstract float XGetter(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill);

		public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
		{
			quality *= curve.Evaluate(XGetter(recipe, surgeon, patient, ingredients, part, bill));
		}
	}
}
