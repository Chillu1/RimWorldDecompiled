using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_ExtractOvum : Recipe_AddHediff
{
	public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
	{
		if (!Find.Storyteller.difficulty.ChildrenAllowed)
		{
			return false;
		}
		if (!(thing is Pawn pawn))
		{
			return false;
		}
		if ((recipe.genderPrerequisite ?? pawn.gender) != pawn.gender)
		{
			return false;
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
		{
			return "CannotPregnant".Translate();
		}
		if (pawn.ageTracker.AgeBiologicalYears < recipe.minAllowedAge)
		{
			return "CannotMustBeAge".Translate(recipe.minAllowedAge);
		}
		if (pawn.Sterile())
		{
			return "CannotSterile".Translate();
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.OvumExtracted))
		{
			return "SurgeryDisableReasonOvumExtracted".Translate();
		}
		return base.AvailableReport(thing, part);
	}

	public override bool CompletableEver(Pawn surgeryTarget)
	{
		return IsValidNow(surgeryTarget, null, ignoreBills: true);
	}

	protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		HumanOvum thing = ThingMaker.MakeThing(ThingDefOf.HumanOvum) as HumanOvum;
		thing.TryGetComp<CompHasPawnSources>().AddSource(pawn);
		if (!GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, null, (IntVec3 x) => x.InBounds(pawn.Map) && x.Standable(pawn.Map) && !x.Fogged(pawn.Map)))
		{
			Log.Error("Could not drop ovum near " + pawn.Position.ToString());
		}
	}
}
