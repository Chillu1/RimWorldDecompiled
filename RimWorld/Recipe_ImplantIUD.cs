using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_ImplantIUD : Recipe_AddHediff
{
	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!(thing is Pawn pawn))
		{
			return false;
		}
		if (pawn.ageTracker.AgeBiologicalYears < 16)
		{
			return false;
		}
		return base.AvailableOnNow(thing, part);
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
		if (pawn.RaceProps.Humanlike)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PregnantHuman);
			if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex == 0)
			{
				PregnancyUtility.TryTerminatePregnancy(pawn);
			}
		}
	}

	public override TaggedString GetConfirmation(Pawn pawn)
	{
		if (pawn.RaceProps.Humanlike)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PregnantHuman);
			if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex == 0)
			{
				return "ConfirmationPawnPregnancyTerminated".Translate(pawn.Named("PAWN"));
			}
		}
		return base.GetConfirmation(pawn);
	}
}
