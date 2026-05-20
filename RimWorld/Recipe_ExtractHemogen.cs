using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_ExtractHemogen : Recipe_Surgery
{
	private const float BloodlossSeverity = 0.45f;

	private const float MinBloodlossForFailure = 0.45f;

	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		Pawn pawn = thing as Pawn;
		if (pawn?.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Hemogenic))
		{
			return false;
		}
		if (pawn != null && !pawn.health.CanBleed)
		{
			return false;
		}
		return base.AvailableOnNow(thing, part);
	}

	public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
	{
		if (thing is Pawn pawn && pawn.DevelopmentalStage.Baby())
		{
			return "TooSmall".Translate();
		}
		return base.AvailableReport(thing, part);
	}

	public override bool CompletableEver(Pawn surgeryTarget)
	{
		if (base.CompletableEver(surgeryTarget))
		{
			return PawnHasEnoughBloodForExtraction(surgeryTarget);
		}
		return false;
	}

	public override void CheckForWarnings(Pawn medPawn)
	{
		base.CheckForWarnings(medPawn);
		if (!PawnHasEnoughBloodForExtraction(medPawn))
		{
			Messages.Message("MessageCannotStartHemogenExtraction".Translate(medPawn.Named("PAWN")), medPawn, MessageTypeDefOf.NeutralEvent, historical: false);
		}
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (!ModLister.CheckBiotech("Hemogen extraction"))
		{
			return;
		}
		if (!PawnHasEnoughBloodForExtraction(pawn))
		{
			Messages.Message("MessagePawnHadNotEnoughBloodToProduceHemogenPack".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
			return;
		}
		Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
		hediff.Severity = 0.45f;
		pawn.health.AddHediff(hediff);
		OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);
		if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
		{
			ReportViolation(pawn, billDoer, pawn.HomeFaction, -1, HistoryEventDefOf.ExtractedHemogenPack);
		}
	}

	protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (!GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.HemogenPack), pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near))
		{
			Log.Error("Could not drop hemogen pack near " + pawn.PositionHeld.ToString());
		}
	}

	private bool PawnHasEnoughBloodForExtraction(Pawn pawn)
	{
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
		if (firstHediffOfDef != null)
		{
			return firstHediffOfDef.Severity < 0.45f;
		}
		return true;
	}
}
