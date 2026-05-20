using Verse;

namespace RimWorld
{
	public class SurgeryOutcomeSuccess : SurgeryOutcome
	{
		public override bool Apply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
		{
			return Rand.Chance(quality);
		}
	}
}
