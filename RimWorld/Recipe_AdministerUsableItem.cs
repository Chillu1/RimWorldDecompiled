using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_AdministerUsableItem : Recipe_Surgery
{
	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (ingredients[0].TryGetComp(out CompUsable comp))
		{
			AcceptanceReport acceptanceReport = comp.CanBeUsedBy(pawn, forced: false, ignoreReserveAndReachable: true);
			if (acceptanceReport.Accepted)
			{
				comp.UsedBy(pawn);
			}
			else if (!string.IsNullOrEmpty(acceptanceReport.Reason))
			{
				comp.SendCannotUseMessage(pawn, acceptanceReport.Reason);
			}
		}
	}

	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!base.AvailableOnNow(thing, part))
		{
			return false;
		}
		if (thing is Pawn pawn)
		{
			Hediff hediff;
			BodyPartRecord part2;
			return HealthUtility.TryGetWorstHealthCondition(pawn, out hediff, out part2);
		}
		return false;
	}

	public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
	{
	}
}
