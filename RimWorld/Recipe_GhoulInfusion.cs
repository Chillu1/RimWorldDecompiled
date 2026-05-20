using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_GhoulInfusion : Recipe_Surgery
{
	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!(thing is Pawn pawn))
		{
			return false;
		}
		if (pawn.IsMutant)
		{
			return false;
		}
		if (!pawn.ageTracker.Adult)
		{
			return false;
		}
		return base.AvailableOnNow(thing, part);
	}

	public override TaggedString GetConfirmation(Pawn pawn)
	{
		if (pawn.IsColonist)
		{
			return "GhoulConfirmation".Translate(pawn.Named("PAWN"));
		}
		return base.GetConfirmation(pawn);
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (!CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
		{
			bool flag = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			Faction homeFaction = pawn.HomeFaction;
			MutantUtility.SetPawnAsMutantInstantly(pawn, MutantDefOf.Ghoul);
			if (pawn.Faction != Faction.OfPlayer)
			{
				pawn.SetFaction(Faction.OfPlayer);
			}
			if (billDoer != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			}
			if (flag)
			{
				ReportViolation(pawn, billDoer, homeFaction, -50, HistoryEventDefOf.GhoulInfusionPerformed);
			}
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ColonyGhouls, OpportunityType.GoodToKnow);
		}
	}
}
