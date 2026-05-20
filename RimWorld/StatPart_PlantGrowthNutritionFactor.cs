using Verse;

namespace RimWorld;

public class StatPart_PlantGrowthNutritionFactor : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (TryGetFactor(req, out var factor))
		{
			val *= factor;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (TryGetFactor(req, out var factor))
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
		if (!(req.Thing is Plant plant))
		{
			factor = 1f;
			return false;
		}
		factor = PlantUtility.NutritionFactorFromGrowth(plant.def, plant.Growth);
		return true;
	}
}
