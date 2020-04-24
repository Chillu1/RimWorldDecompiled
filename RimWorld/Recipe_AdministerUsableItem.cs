using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Recipe_AdministerUsableItem : Recipe_Surgery
	{
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			ingredients[0].TryGetComp<CompUsable>().UsedBy(pawn);
		}

		public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
		{
		}
	}
}
