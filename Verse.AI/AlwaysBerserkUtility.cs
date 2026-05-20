using RimWorld;

namespace Verse.AI;

public static class AlwaysBerserkUtility
{
	public static bool TryTriggerBerserkBloodRage(Pawn pawn)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.BloodRage))
		{
			return pawn.mindState.mentalBreaker.TryDoMentalBreak("MentalBreakReason_BloodRage".Translate(), MentalBreakDefOf.Berserk);
		}
		return false;
	}

	public static bool TryTriggerBerserkFrenzyInducer(Pawn pawn)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FrenzyField);
		if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex > 0)
		{
			return pawn.mindState.mentalBreaker.TryDoMentalBreak("MentalBreakReason_FrenzyInducer".Translate(), MentalBreakDefOf.Berserk);
		}
		return false;
	}
}
