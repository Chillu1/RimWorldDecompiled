using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SurgeryOutcome_HediffWithDuration : SurgeryOutcome
	{
		public HediffDef hediff;

		public SimpleCurve qualityToDurationDaysCurve;

		public override bool Apply(float quality, RecipeDef recipe, Pawn surgeon, Pawn patient, BodyPartRecord part)
		{
			if (Rand.Chance(chance))
			{
				Hediff hd = HediffMaker.MakeHediff(hediff, patient, applyEffectsToPart ? part : null);
				hd.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.RoundToInt(qualityToDurationDaysCurve.Evaluate(quality) * 60000f);
				patient.health.AddHediff(hd);
				return true;
			}
			return false;
		}
	}
}
