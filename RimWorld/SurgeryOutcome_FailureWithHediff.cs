using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class SurgeryOutcome_FailureWithHediff : SurgeryOutcome_Failure
{
	public List<RecipeDef> applyToRecipes;

	public HediffDef failedHediff;

	protected override bool CanApply(RecipeDef recipe)
	{
		if (failedHediff == null)
		{
			return false;
		}
		if (!base.CanApply(recipe))
		{
			return false;
		}
		if (!applyToRecipes.NullOrEmpty())
		{
			return applyToRecipes.Contains(recipe);
		}
		return true;
	}

	protected override void PostDamagedApplied(Pawn patient)
	{
		patient.health.AddHediff(failedHediff);
	}
}
