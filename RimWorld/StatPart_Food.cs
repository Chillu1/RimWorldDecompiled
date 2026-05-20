using System;
using Verse;

namespace RimWorld;

public class StatPart_Food : StatPart
{
	public float factorStarving = 1f;

	public float factorUrgentlyHungry = 1f;

	public float factorHungry = 1f;

	public float factorFed = 1f;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn pawn && pawn.needs.food != null)
		{
			val *= FoodMultiplier(pawn.needs.food.CurCategory);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn pawn && pawn.needs.food != null)
		{
			return pawn.needs.food.CurCategory.GetLabel() + ": x" + FoodMultiplier(pawn.needs.food.CurCategory).ToStringPercent();
		}
		return null;
	}

	private float FoodMultiplier(HungerCategory hunger)
	{
		return hunger switch
		{
			HungerCategory.Starving => factorStarving, 
			HungerCategory.UrgentlyHungry => factorUrgentlyHungry, 
			HungerCategory.Hungry => factorHungry, 
			HungerCategory.Fed => factorFed, 
			_ => throw new InvalidOperationException(), 
		};
	}
}
