using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class WorkGiverUtility
	{
		public static Job HaulStuffOffBillGiverJob(Pawn pawn, IBillGiver giver, Thing thingToIgnore)
		{
			foreach (IntVec3 ingredientStackCell in giver.IngredientStackCells)
			{
				Thing thing = pawn.Map.thingGrid.ThingAt(ingredientStackCell, ThingCategory.Item);
				if (thing != null && thing != thingToIgnore)
				{
					return HaulAIUtility.HaulAsideJobFor(pawn, thing);
				}
			}
			return null;
		}
	}
}
