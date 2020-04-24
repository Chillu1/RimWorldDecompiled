using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatPart_PlantGrowthNutritionFactor : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (TryGetFactor(req, out float factor))
			{
				val *= factor;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (TryGetFactor(req, out float factor))
			{
				Plant plant = (Plant)req.Thing;
				TaggedString taggedString = "StatsReport_PlantGrowth".Translate(plant.Growth.ToStringPercent()) + ": x" + factor.ToStringPercent();
				if (!plant.def.plant.Sowable)
				{
					taggedString += " (" + "StatsReport_PlantGrowth_Wild".Translate() + ")";
				}
				return taggedString;
			}
			return null;
		}

		private bool TryGetFactor(StatRequest req, out float factor)
		{
			if (!req.HasThing)
			{
				factor = 1f;
				return false;
			}
			Plant plant = req.Thing as Plant;
			if (plant == null)
			{
				factor = 1f;
				return false;
			}
			if (plant.def.plant.Sowable)
			{
				factor = plant.Growth;
				return true;
			}
			factor = Mathf.Lerp(0.5f, 1f, plant.Growth);
			return true;
		}
	}
}
