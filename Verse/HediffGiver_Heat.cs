using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse
{
	public class HediffGiver_Heat : HediffGiver
	{
		private const int BurnCheckInterval = 420;

		public static readonly string MemoPawnBurnedByAir = "PawnBurnedByAir";

		public static readonly SimpleCurve TemperatureOverageAdjustmentCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(25f, 25f),
			new CurvePoint(50f, 40f),
			new CurvePoint(100f, 60f),
			new CurvePoint(200f, 80f),
			new CurvePoint(400f, 100f),
			new CurvePoint(4000f, 1000f)
		};

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			float ambientTemperature = pawn.AmbientTemperature;
			FloatRange floatRange = pawn.ComfortableTemperatureRange();
			FloatRange floatRange2 = pawn.SafeTemperatureRange();
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
			if (ambientTemperature > floatRange2.max)
			{
				float x = ambientTemperature - floatRange2.max;
				x = TemperatureOverageAdjustmentCurve.Evaluate(x);
				float a = x * 6.45E-05f;
				a = Mathf.Max(a, 0.000375f);
				HealthUtility.AdjustSeverity(pawn, hediff, a);
			}
			else if (firstHediffOfDef != null && ambientTemperature < floatRange.max)
			{
				float value = firstHediffOfDef.Severity * 0.027f;
				value = Mathf.Clamp(value, 0.0015f, 0.015f);
				firstHediffOfDef.Severity -= value;
			}
			if (pawn.Dead || !pawn.IsNestedHashIntervalTick(60, 420))
			{
				return;
			}
			float num = floatRange.max + 150f;
			if (!(ambientTemperature > num))
			{
				return;
			}
			float x2 = ambientTemperature - num;
			x2 = TemperatureOverageAdjustmentCurve.Evaluate(x2);
			int num2 = Mathf.Max(GenMath.RoundRandom(x2 * 0.06f), 3);
			DamageInfo dinfo = new DamageInfo(DamageDefOf.Burn, num2);
			dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			pawn.TakeDamage(dinfo);
			if (pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeed();
				if (MessagesRepeatAvoider.MessageShowAllowed("PawnBeingBurned", 60f))
				{
					Messages.Message("MessagePawnBeingBurned".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.ThreatSmall);
				}
			}
			pawn.GetLord()?.ReceiveMemo(MemoPawnBurnedByAir);
		}
	}
}
