using Verse;

namespace RimWorld
{
	public class StatPart_UnfinishedThingIngredientsMass : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (TryGetValue(req, out float value))
			{
				val += value;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (TryGetValue(req, out float value))
			{
				return "StatsReport_IngredientsMass".Translate() + ": " + value.ToStringMassOffset();
			}
			return null;
		}

		private bool TryGetValue(StatRequest req, out float value)
		{
			UnfinishedThing unfinishedThing = req.Thing as UnfinishedThing;
			if (unfinishedThing == null)
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
}
