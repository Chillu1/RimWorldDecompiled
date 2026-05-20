using System;

namespace RimWorld.Planet;

public static class OverallPopulationUtility
{
	private static int cachedEnumValuesCount = -1;

	private const float ScaleFactor_AlmostNone = 0.1f;

	private const float ScaleFactor_Little = 0.4f;

	private const float ScaleFactor_LittleBitLess = 0.7f;

	private const float ScaleFactor_Normal = 1f;

	private const float ScaleFactor_LittleBitMore = 1.5f;

	private const float ScaleFactor_High = 2f;

	private const float ScaleFactor_VeryHigh = 2.75f;

	public static int EnumValuesCount
	{
		get
		{
			if (cachedEnumValuesCount < 0)
			{
				cachedEnumValuesCount = Enum.GetNames(typeof(OverallPopulation)).Length;
			}
			return cachedEnumValuesCount;
		}
	}

	public static float GetScaleFactor(this OverallPopulation population)
	{
		return population switch
		{
			OverallPopulation.AlmostNone => 0.1f, 
			OverallPopulation.Little => 0.4f, 
			OverallPopulation.LittleBitLess => 0.7f, 
			OverallPopulation.Normal => 1f, 
			OverallPopulation.LittleBitMore => 1.5f, 
			OverallPopulation.High => 2f, 
			OverallPopulation.VeryHigh => 2.75f, 
			_ => 1f, 
		};
	}
}
