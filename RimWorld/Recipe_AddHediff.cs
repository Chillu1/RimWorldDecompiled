using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_AddHediff : Recipe_Surgery
{
	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		return IsValidNow(thing, part);
	}

	public bool IsValidNow(Thing thing, BodyPartRecord part = null, bool ignoreBills = false)
	{
		if (!base.AvailableOnNow(thing, part))
		{
			return false;
		}
		if (!(thing is Pawn pawn))
		{
			return false;
		}
		if (part != null && (pawn.health.WouldDieAfterAddingHediff(recipe.addsHediff, part, 1f) || pawn.health.WouldLosePartAfterAddingHediff(recipe.addsHediff, part, 1f)))
		{
			return false;
		}
		if (pawn.health.hediffSet.HasHediff(recipe.addsHediff))
		{
			return false;
		}
		if (!ignoreBills && pawn.BillStack.Bills.Any((Bill b) => b.recipe == recipe))
		{
			return false;
		}
		if (pawn.health.hediffSet.hediffs.Any((Hediff x) => !recipe.CompatibleWithHediff(x.def)))
		{
			return false;
		}
		return true;
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (billDoer != null)
		{
			if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
			{
				return;
			}
			TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
		}
		pawn.health.AddHediff(recipe.addsHediff, part);
		OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);
		if (IsViolationOnPawn(pawn, part, Faction.OfPlayerSilentFail))
		{
			ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
		}
	}
}
