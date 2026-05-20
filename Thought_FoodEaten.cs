using RimWorld;
using Verse;

public class Thought_FoodEaten : Thought_Memory
{
	private string foodThoughtDescription;

	public override string Description => base.Description + "\n\n" + foodThoughtDescription;

	public void SetFood(Thing food)
	{
		CompIngredients compIngredients = food.TryGetComp<CompIngredients>();
		foodThoughtDescription = "ThoughtFoodEatenFood".Translate() + ": " + food.def.LabelCap;
		if (compIngredients != null && compIngredients.ingredients.Count > 0)
		{
			foodThoughtDescription += " (" + "ThoughtFoodEatenIngredients".Translate() + ": " + compIngredients.GetIngredientsString(includeMergeCompatibility: false, out var _) + ")";
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref foodThoughtDescription, "foodThoughtDescription");
	}
}
