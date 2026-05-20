using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityCounsel : CompProperties_AbilityEffect
	{
		public float minMoodOffset = -10f;

		[MustTranslate]
		public string successMessage;

		[MustTranslate]
		public string successMessageNoNegativeThought;

		[MustTranslate]
		public string failMessage;

		public ThoughtDef failedThoughtRecipient;

		public CompProperties_AbilityCounsel()
		{
			compClass = typeof(CompAbilityEffect_Counsel);
		}
	}
}
