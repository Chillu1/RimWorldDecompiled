using System;
using Verse;

namespace RimWorld;

public class CompBiosculpterPod_AgeReversalCycle : CompBiosculpterPod_Cycle
{
	public override string Description(Pawn tunedFor)
	{
		int num = 3600000;
		if (tunedFor != null)
		{
			num *= (int)tunedFor.ageTracker.AdultAgingMultiplier;
		}
		return base.Description(tunedFor).Formatted(num.ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false).Named("DURATION"));
	}

	public override void CycleCompleted(Pawn pawn)
	{
		int num = (int)(3600000f * pawn.ageTracker.AdultAgingMultiplier);
		long val = (long)(3600000f * pawn.ageTracker.AdultMinAge);
		pawn.ageTracker.AgeBiologicalTicks = Math.Max(val, pawn.ageTracker.AgeBiologicalTicks - num);
		pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
		int num2 = (int)(pawn.ageTracker.AgeReversalDemandedDeadlineTicks / 60000);
		string text = "BiosculpterAgeReversalCompletedMessage".Translate(pawn.Named("PAWN"));
		Ideo ideo = pawn.Ideo;
		if (ideo != null && ideo.HasPrecept(PreceptDefOf.AgeReversal_Demanded))
		{
			text += " " + "AgeReversalExpectationDeadline".Translate(pawn.Named("PAWN"), num2.Named("DEADLINE")).CapitalizeFirst();
		}
		Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent);
	}
}
