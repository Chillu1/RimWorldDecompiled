using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_TerminatePregnancy : Recipe_Surgery
{
	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!(thing is Pawn pawn))
		{
			return false;
		}
		if (PregnancyUtility.GetPregnancyHediff(pawn) == null)
		{
			return false;
		}
		return true;
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (PregnancyUtility.GetPregnancyHediff(pawn) != null && (billDoer == null || !CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) && PregnancyUtility.TryTerminatePregnancy(pawn) && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			if (billDoer != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			}
			Messages.Message("MessagePregnancyTerminated".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
			if (IsViolationOnPawn(pawn, part, Faction.OfPlayerSilentFail))
			{
				ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
			}
		}
	}
}
