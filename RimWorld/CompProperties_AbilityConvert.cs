using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityConvert : CompProperties_AbilityEffect
	{
		[MustTranslate]
		public string successMessage;

		[MustTranslate]
		public string failMessage;

		public ThoughtDef failedThoughtInitiator;

		public ThoughtDef failedThoughtRecipient;

		public float convertPowerFactor = -1f;

		public CompProperties_AbilityConvert()
		{
			compClass = typeof(CompAbilityEffect_Convert);
		}

		public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
		{
			if (convertPowerFactor < 0f)
			{
				yield return "convertPowerFactor not set";
			}
		}
	}
}
