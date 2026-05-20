using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeComp_ClampToRange : SurgeryOutcomeComp
	{
		public FloatRange range;

		public override void AffectQuality(RecipeDef recipe, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill, ref float quality)
		{
			quality = range.ClampToRange(quality);
		}
	}
}
