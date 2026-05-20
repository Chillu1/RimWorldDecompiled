using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public class HediffGiver_Hypothermia : HediffGiver
{
	public HediffDef hediffInsectoid;

	public override void OnIntervalPassed(Pawn pawn, Hediff cause)
	{
		float ambientTemperature = pawn.AmbientTemperature;
		FloatRange floatRange = pawn.ComfortableTemperatureRange();
		FloatRange floatRange2 = pawn.SafeTemperatureRange();
		HediffSet hediffSet = pawn.health.hediffSet;
		HediffDef hediffDef = ((pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid) ? hediffInsectoid : hediff);
		Hediff firstHediffOfDef = hediffSet.GetFirstHediffOfDef(hediffDef);
		if (ambientTemperature < floatRange2.min)
		{
			float a = Mathf.Abs(ambientTemperature - floatRange2.min) * 6.45E-05f;
			a = Mathf.Max(a, 0.00075f);
			HealthUtility.AdjustSeverity(pawn, hediffDef, a);
			if (pawn.Dead)
			{
				return;
			}
		}
		if (firstHediffOfDef == null)
		{
			return;
		}
		if (ambientTemperature > floatRange.min || !pawn.SpawnedOrAnyParentSpawned || pawn.PositionHeld.GetTerrain(pawn.MapHeld).heatPerTick > 0f)
		{
			float value = firstHediffOfDef.Severity * 0.027f;
			value = Mathf.Clamp(value, 0.0015f, 0.015f);
			firstHediffOfDef.Severity -= value;
		}
		else if (pawn.RaceProps.FleshType != FleshTypeDefOf.Insectoid && ambientTemperature < 0f && firstHediffOfDef.Severity > 0.37f)
		{
			float num = 0.025f * firstHediffOfDef.Severity;
			if (Rand.Value < num && pawn.RaceProps.body.AllPartsVulnerableToFrostbite.Where((BodyPartRecord x) => !hediffSet.PartIsMissing(x)).TryRandomElementByWeight((BodyPartRecord x) => x.def.frostbiteVulnerability, out var result))
			{
				int num2 = Mathf.CeilToInt((float)result.def.hitPoints * 0.5f);
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Frostbite, num2, 0f, -1f, null, result);
				pawn.TakeDamage(dinfo);
			}
		}
	}
}
