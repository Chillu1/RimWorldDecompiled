using UnityEngine;

namespace Verse
{
	public sealed class IngredientCount
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

		public int CountRequiredOfFor(ThingDef thingDef, RecipeDef recipe)
		{
			float num = recipe.IngredientValueGetter.ValuePerUnitOf(thingDef);
			return Mathf.CeilToInt(count / num);
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
	}
}
