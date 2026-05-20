namespace Verse;

public static class WeatherBuildupUtility
{
	public static WeatherBuildupCategory GetBuildupCategory(float depth)
	{
		if (depth < 0.03f)
		{
			return WeatherBuildupCategory.None;
		}
		if (depth < 0.25f)
		{
			return WeatherBuildupCategory.Dusting;
		}
		if (depth < 0.5f)
		{
			return WeatherBuildupCategory.Thin;
		}
		if (depth < 0.75f)
		{
			return WeatherBuildupCategory.Medium;
		}
		return WeatherBuildupCategory.Thick;
	}

	public static string GetSnowDescription(WeatherBuildupCategory category)
	{
		return category switch
		{
			WeatherBuildupCategory.None => "SnowNone".Translate(), 
			WeatherBuildupCategory.Dusting => "SnowDusting".Translate(), 
			WeatherBuildupCategory.Thin => "SnowThin".Translate(), 
			WeatherBuildupCategory.Medium => "SnowMedium".Translate(), 
			WeatherBuildupCategory.Thick => "SnowThick".Translate(), 
			_ => "Unknown snow", 
		};
	}

	public static string GetSandDescription(WeatherBuildupCategory category)
	{
		return category switch
		{
			WeatherBuildupCategory.None => "SandNone".Translate(), 
			WeatherBuildupCategory.Dusting => "SandDusting".Translate(), 
			WeatherBuildupCategory.Thin => "SandThin".Translate(), 
			WeatherBuildupCategory.Medium => "SandMedium".Translate(), 
			WeatherBuildupCategory.Thick => "SandThick".Translate(), 
			_ => "Unknown sand", 
		};
	}

	public static int MovementTicksAddOn(WeatherBuildupCategory category)
	{
		return category switch
		{
			WeatherBuildupCategory.None => 0, 
			WeatherBuildupCategory.Dusting => 0, 
			WeatherBuildupCategory.Thin => 4, 
			WeatherBuildupCategory.Medium => 8, 
			WeatherBuildupCategory.Thick => 12, 
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

	public static void AddSandRadial(IntVec3 center, Map map, float radius, float depth)
	{
		int num = GenRadial.NumCellsInRadius(radius);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = center + GenRadial.RadialPattern[i];
			if (intVec.InBounds(map))
			{
				float lengthHorizontal = (center - intVec).LengthHorizontal;
				float num2 = 1f - lengthHorizontal / radius;
				map.sandGrid.AddDepth(intVec, num2 * depth);
			}
		}
	}
}
