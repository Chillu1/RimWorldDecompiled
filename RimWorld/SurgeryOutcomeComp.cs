using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeComp
	{
		public virtual bool Affects(RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
		{
			return true;
		}

		public virtual void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
		{
		}

		public virtual void PreApply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
		}
	}
}
