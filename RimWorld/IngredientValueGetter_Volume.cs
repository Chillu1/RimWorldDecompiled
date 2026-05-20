using System.Linq;
using Verse;

namespace RimWorld;

public class IngredientValueGetter_Volume : IngredientValueGetter
{
	public override float ValuePerUnitOf(ThingDef t)
	{
		if (t.IsStuff)
		{
			return t.VolumePerUnit;
		}
		return 1f;
	}

	public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
	{
		if (!ing.filter.AllowedThingDefs.Any((ThingDef td) => td.smallVolume) || ing.filter.AllowedThingDefs.Any((ThingDef td) => td.smallVolume && !r.GetPremultipliedSmallIngredients().Contains(td)))
		{
			return ing.GetBaseCount() + "x " + ing.filter.Summary;
		}
		return ing.GetBaseCount() * 10f + "x " + ing.filter.Summary;
	}

	public override string ExtraDescriptionLine(RecipeDef r)
	{
		if (r.ingredients.Any((IngredientCount ing) => ing.filter.AllowedThingDefs.Any((ThingDef td) => td.smallVolume && !r.GetPremultipliedSmallIngredients().Contains(td))))
		{
			return "BillRequiresMayVary".Translate(10.ToStringCached());
		}
		return null;
	}
}
