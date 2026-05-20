using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_ImplantXenogerm : Recipe_Surgery
{
	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!base.AvailableOnNow(thing, part))
		{
			return false;
		}
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!(thing is Pawn { Spawned: not false } pawn))
		{
			return false;
		}
		List<Thing> list = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Xenogerm);
		if (list.Any())
		{
			foreach (Thing item in list)
			{
				if (!item.IsForbidden(pawn) && !item.Position.Fogged(pawn.Map))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (ModLister.CheckBiotech("xenogerm implanting") && !CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
		{
			if (bill.xenogerm != null)
			{
				GeneUtility.ImplantXenogermItem(pawn, bill.xenogerm);
			}
			if (IsViolationOnPawn(pawn, part, Faction.OfPlayer))
			{
				ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
			}
			if (ModsConfig.IdeologyActive)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.InstalledProsthetic, billDoer.Named(HistoryEventArgsNames.Doer)));
			}
		}
	}
}
