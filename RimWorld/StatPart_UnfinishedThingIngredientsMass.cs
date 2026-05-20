using Verse;

namespace RimWorld;

public class StatPart_UnfinishedThingIngredientsMass : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetValue(req, out var value))
		{
			val += value;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetValue(req, out var value))
		{
			return "StatsReport_IngredientsMass".Translate() + ": " + value.ToStringMassOffset();
		}
		return null;
	}

	private bool TryGetValue(StatRequest req, out float value)
	{
		if (!(req.Thing is UnfinishedThing unfinishedThing))
		{
			value = 0f;
			return false;
		}
		float num = 0f;
		for (int i = 0; i < unfinishedThing.ingredients.Count; i++)
		{
			num += unfinishedThing.ingredients[i].GetStatValue(StatDefOf.Mass) * (float)unfinishedThing.ingredients[i].stackCount;
		}
		value = num;
		return true;
	}
}
