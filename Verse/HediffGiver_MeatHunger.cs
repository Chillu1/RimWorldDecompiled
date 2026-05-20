using RimWorld;

namespace Verse;

public class HediffGiver_MeatHunger : HediffGiver
{
	private const float BaseMeatHungerSeverityIncreasePerDay = 0.9f;

	private const float BaseMeatHungerSeverityDecreasePerDay = 24f;

	private const float BaseMeatHungerSeverityIncreasePerInterval = 0.00090000004f;

	private const float BaseMeatHungerSeverityDecreasePerInterval = 0.024f;

	public override void OnIntervalPassed(Pawn pawn, Hediff cause)
	{
		if (pawn.needs != null && pawn.needs.TryGetNeed(NeedDefOf.Food, out var need) && need.CurLevel <= 0f)
		{
			HealthUtility.AdjustSeverity(pawn, hediff, 0.00090000004f);
		}
		else
		{
			HealthUtility.AdjustSeverity(pawn, hediff, -0.024f);
		}
	}
}
