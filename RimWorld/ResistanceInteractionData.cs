using Verse;

namespace RimWorld
{
	public class ResistanceInteractionData : IExposable
	{
		public float resistanceReduction;

		public float initiatorNegotiationAbilityFactor;

		public float recruiteeMoodFactor;

		public string initiatorName;

		public float recruiterOpinionFactor;

		public void ExposeData()
		{
			Scribe_Values.Look(ref resistanceReduction, "resistanceReduction", 0f);
			Scribe_Values.Look(ref initiatorNegotiationAbilityFactor, "initiatorNegotiationAbilityFactor", 0f);
			Scribe_Values.Look(ref recruiteeMoodFactor, "recruiteeMoodFactor", 0f);
			Scribe_Values.Look(ref initiatorName, "initiatorName");
			Scribe_Values.Look(ref recruiterOpinionFactor, "recruiterOpinionFactor", 0f);
		}
	}
}
