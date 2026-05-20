using RimWorld;
using RimWorld.Planet;

namespace Verse;

public class Hediff_DuplicateSickness : HediffWithComps
{
	private const float LetterSeverityThreshold = 0.2f;

	private const float WorldPawnKillSeverityThreshold = 0.7f;

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Duplicate sickness"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void PostTickInterval(int delta)
	{
		base.PostTickInterval(delta);
		if (Severity >= 0.2f && !Find.History.duplicateSicknessDiscovered)
		{
			Find.LetterStack.ReceiveLetter("DuplicateSicknessLabel".Translate(), "DuplicateSicknessText".Translate(pawn), LetterDefOf.NegativeEvent, pawn);
			Find.History.Notify_DuplicateSicknessDiscovered();
		}
		if (pawn.IsWorldPawn() && Severity >= 0.7f && !pawn.Dead)
		{
			pawn.Kill(null, null);
		}
	}
}
