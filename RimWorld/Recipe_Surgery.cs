using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_Surgery : RecipeWorker
{
	protected bool CheckSurgeryFail(Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
	{
		if (recipe.surgeryOutcomeEffect == null)
		{
			return false;
		}
		SurgeryOutcome outcome = recipe.surgeryOutcomeEffect.GetOutcome(recipe, surgeon, patient, ingredients, part, bill);
		if (outcome != null && outcome.failure)
		{
			if (recipe.addsHediffOnFailure != null)
			{
				patient.health.AddHediff(recipe.addsHediffOnFailure, part);
			}
			return true;
		}
		return false;
	}

	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!(thing is Pawn pawn))
		{
			return false;
		}
		if ((recipe.genderPrerequisite ?? pawn.gender) != pawn.gender)
		{
			return false;
		}
		if (recipe.mustBeFertile && pawn.Sterile())
		{
			return false;
		}
		if (!recipe.allowedForQuestLodgers && pawn.IsQuestLodger())
		{
			return false;
		}
		if (recipe.minAllowedAge > 0 && pawn.ageTracker.AgeBiologicalYears < recipe.minAllowedAge)
		{
			return false;
		}
		if (recipe.developmentalStageFilter.HasValue && !recipe.developmentalStageFilter.Value.Has(pawn.DevelopmentalStage))
		{
			return false;
		}
		if (recipe.humanlikeOnly && !pawn.RaceProps.Humanlike)
		{
			return false;
		}
		if (ModsConfig.AnomalyActive)
		{
			if (recipe.mutantBlacklist != null && pawn.IsMutant && recipe.mutantBlacklist.Contains(pawn.mutant.Def))
			{
				return false;
			}
			if (recipe.mutantPrerequisite != null && (!pawn.IsMutant || !recipe.mutantPrerequisite.Contains(pawn.mutant.Def)))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool CompletableEver(Pawn surgeryTarget)
	{
		return true;
	}

	protected virtual void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
	}
}
