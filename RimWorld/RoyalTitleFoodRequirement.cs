using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct RoyalTitleFoodRequirement
	{
		public FoodPreferability minQuality;

		public FoodTypeFlags allowedTypes;

		public List<ThingDef> allowedDefs;

		public bool Defined => minQuality != FoodPreferability.Undefined;

		public bool Acceptable(ThingDef food)
		{
			if (food.ingestible == null)
			{
				return false;
			}
			if (allowedDefs.Contains(food))
			{
				return true;
			}
			if (allowedTypes != 0 && (allowedTypes & food.ingestible.foodType) != 0)
			{
				return true;
			}
			return (int)food.ingestible.preferability >= (int)minQuality;
		}
	}
}
