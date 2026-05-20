using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_SurgicalInspection : Recipe_Surgery
{
	private const int AnestheticTicks = 60000;

	private const int CutDamage = 4;

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
		{
			return;
		}
		string desc;
		SurgicalInspectionOutcome surgicalInspectionOutcome = pawn.DoSurgicalInspection(billDoer, out desc);
		if (surgicalInspectionOutcome != SurgicalInspectionOutcome.DetectedNoLetter)
		{
			TaggedString label = "LetterSurgicallyInspectedLabel".Translate();
			TaggedString text = "LetterSurgicallyInspectedHeader".Translate(billDoer.Named("DOCTOR"), pawn.Named("PATIENT"));
			if (surgicalInspectionOutcome == SurgicalInspectionOutcome.Nothing)
			{
				text += " " + "LetterSurgicallyInspectedNothing".Translate(billDoer.Named("DOCTOR"), pawn.Named("PATIENT")).CapitalizeFirst();
			}
			else
			{
				text += desc;
			}
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, pawn);
		}
		if (billDoer != null)
		{
			TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
		}
		pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 4f));
		HediffComp_Disappears hediffComp_Disappears = pawn.health.AddHediff(HediffDefOf.Anesthetic).TryGetComp<HediffComp_Disappears>();
		hediffComp_Disappears.disappearsAfterTicks = 60000;
		hediffComp_Disappears.ticksToDisappear = 60000;
	}
}
