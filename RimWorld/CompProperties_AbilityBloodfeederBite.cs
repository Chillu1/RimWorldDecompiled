using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityBloodfeederBite : CompProperties_AbilityEffect
	{
		public float hemogenGain;

		public ThoughtDef thoughtDefToGiveTarget;

		public ThoughtDef opinionThoughtDefToGiveTarget;

		public float resistanceGain;

		public float nutritionGain = 0.1f;

		public float targetBloodLoss = 0.4499f;

		public IntRange bloodFilthToSpawnRange;

		public CompProperties_AbilityBloodfeederBite()
		{
			compClass = typeof(CompAbilityEffect_BloodfeederBite);
		}

		public override IEnumerable<string> ExtraStatSummary()
		{
			yield return "AbilityHemogenGain".Translate() + ": " + (hemogenGain * 100f).ToString("F0");
		}
	}
}
