using RimWorld;
using UnityEngine;

namespace Verse;

public static class ToxicUtility
{
	public const int CheckInterval = 3451;

	private const float ToxicPerDay = 0.4f;

	public static void PawnToxicTickInterval(Pawn pawn, int delta)
	{
		if (pawn.IsHashIntervalTick(3451, delta) && pawn.Spawned)
		{
			float num = pawn.Position.GetTerrain(pawn.Map).toxicBuildupFactor;
			if (ModsConfig.BiotechActive && pawn.Position.IsPolluted(pawn.Map))
			{
				num += 1f;
			}
			if (num > 0f)
			{
				DoPawnToxicDamage(pawn, num);
			}
		}
	}

	public static void DoAirbornePawnToxicDamage(Pawn p, float extraFactor = 1f)
	{
		if (!p.Spawned || !p.Position.Roofed(p.Map))
		{
			DoPawnToxicDamage(p, extraFactor);
		}
	}

	public static void DoPawnToxicDamage(Pawn p, float extraFactor = 1f)
	{
		float num = 0.023006668f;
		num *= Mathf.Max(1f - p.GetStatValue(StatDefOf.ToxicResistance), 0f);
		num *= Mathf.Max(1f - p.GetStatValue(StatDefOf.ToxicEnvironmentResistance), 0f);
		num *= extraFactor;
		if (num != 0f)
		{
			float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 0x46EDC5D));
			num *= num2;
			HealthUtility.AdjustSeverity(p, HediffDefOf.ToxicBuildup, num);
		}
	}
}
