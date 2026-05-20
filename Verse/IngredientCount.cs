using System;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public sealed class IngredientCount : IExposable
{
	public ThingFilter filter = new ThingFilter();

	private float count = 1f;

	public bool IsFixedIngredient => filter.AllowedDefCount == 1;

	public ThingDef FixedIngredient
	{
		get
		{
			if (!IsFixedIngredient)
			{
				Log.Error("Called for SingleIngredient on an IngredientCount that is not IsSingleIngredient: " + this);
			}
			return filter.AnyAllowedDef;
		}
	}

	public string Summary => count + "x " + filter.Summary;

	public string SummaryFilterFirst => filter.Summary.CapitalizeFirst() + " x" + count;

	public string SummaryFor(RecipeDef recipe)
	{
		return CountFor(recipe) + "x " + filter.Summary;
	}

	public float CountFor(RecipeDef recipe)
	{
		float num = GetBaseCount();
		ThingDef thingDef = filter.AllowedThingDefs.FirstOrDefault((ThingDef x) => recipe.fixedIngredientFilter.Allows(x) && !x.smallVolume) ?? filter.AllowedThingDefs.FirstOrDefault((ThingDef x) => recipe.fixedIngredientFilter.Allows(x));
		if (thingDef != null)
		{
			float num2 = recipe.IngredientValueGetter.ValuePerUnitOf(thingDef);
			if (Math.Abs(num2) > float.Epsilon)
			{
				num /= num2;
			}
		}
		return num;
	}

	public int CountRequiredOfFor(ThingDef thingDef, RecipeDef recipe, Bill bill = null)
	{
		float num = recipe.IngredientValueGetter.ValuePerUnitOf(thingDef);
		return Mathf.CeilToInt(((bill == null) ? count : recipe.Worker.GetIngredientCount(this, bill)) / num);
	}

	public float GetBaseCount()
	{
		return count;
	}

	public void SetBaseCount(float count)
	{
		this.count = count;
	}

	public void ResolveReferences()
	{
		filter.ResolveReferences();
	}

	public override string ToString()
	{
		return "(" + Summary + ")";
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref filter, "filter");
		Scribe_Values.Look(ref count, "count", 0f);
	}
}
