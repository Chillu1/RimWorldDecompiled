using RimWorld;

namespace Verse;

public class Hediff_Alcohol : Hediff_High
{
	private const int HangoverCheckInterval = 300;

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (CurStageIndex >= 3 && pawn.IsHashIntervalTick(300, delta) && HangoverSusceptible(pawn))
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hangover);
			if (firstHediffOfDef != null)
			{
				firstHediffOfDef.Severity = 1f;
				return;
			}
			firstHediffOfDef = HediffMaker.MakeHediff(HediffDefOf.Hangover, pawn);
			firstHediffOfDef.Severity = 1f;
			pawn.health.AddHediff(firstHediffOfDef);
		}
	}

	private bool HangoverSusceptible(Pawn pawn)
	{
		return true;
	}
}
