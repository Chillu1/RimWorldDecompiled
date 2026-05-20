using Verse;

namespace RimWorld;

public class IngredientValueGetter_Nutrition : IngredientValueGetter
{
	public override float ValuePerUnitOf(ThingDef t)
	{
		if (!t.IsNutritionGivingIngestible)
		{
			return 0f;
		}
		return t.GetStatValueAbstract(StatDefOf.Nutrition);
	}

	public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
	{
		return ing.GetBaseCount() + "x " + "BillNutrition".Translate() + " (" + ing.filter.Summary + ")";
	}
}
