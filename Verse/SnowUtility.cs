namespace Verse
{
	public static class SnowUtility
	{
		public static SnowCategory GetSnowCategory(float snowDepth)
		{
			if (snowDepth < 0.03f)
			{
				return SnowCategory.None;
			}
			if (snowDepth < 0.25f)
			{
				return SnowCategory.Dusting;
			}
			if (snowDepth < 0.5f)
			{
				return SnowCategory.Thin;
			}
			if (snowDepth < 0.75f)
			{
				return SnowCategory.Medium;
			}
			return SnowCategory.Thick;
		}

		public static string GetDescription(SnowCategory category)
		{
			return category switch
			{
				SnowCategory.None => "SnowNone".Translate(), 
				SnowCategory.Dusting => "SnowDusting".Translate(), 
				SnowCategory.Thin => "SnowThin".Translate(), 
				SnowCategory.Medium => "SnowMedium".Translate(), 
				SnowCategory.Thick => "SnowThick".Translate(), 
				_ => "Unknown snow", 
			};
		}

		public static int MovementTicksAddOn(SnowCategory category)
		{
			return category switch
			{
				SnowCategory.None => 0, 
				SnowCategory.Dusting => 0, 
				SnowCategory.Thin => 4, 
				SnowCategory.Medium => 8, 
				SnowCategory.Thick => 12, 
				_ => 0, 
			};
		}

		public static void AddSnowRadial(IntVec3 center, Map map, float radius, float depth)
		{
			int num = GenRadial.NumCellsInRadius(radius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					float lengthHorizontal = (center - intVec).LengthHorizontal;
					float num2 = 1f - lengthHorizontal / radius;
					map.snowGrid.AddDepth(intVec, num2 * depth);
				}
			}
		}
	}
}
