using System;

namespace RimWorld.Planet;

public static class LandmarkDensityUtility
{
	private static int cachedEnumValuesCount = -1;

	private const float ScaleFactor_Sparse = 0.05f;

	private const float ScaleFactor_SlightlyMoreSparse = 0.33f;

	private const float ScaleFactor_SlightlySparse = 0.66f;

	private const float ScaleFactor_Normal = 1f;

	private const float ScaleFactor_SlightlyCrowded = 1.5f;

	private const float ScaleFactor_SlightlyMoreCrowded = 2f;

	private const float ScaleFactor_Crowded = 3f;

	public static int EnumValuesCount
	{
		get
		{
			if (cachedEnumValuesCount < 0)
			{
				cachedEnumValuesCount = Enum.GetNames(typeof(LandmarkDensity)).Length;
			}
			return cachedEnumValuesCount;
		}
	}

	public static float GetScaleFactor(this LandmarkDensity density)
	{
		return density switch
		{
			LandmarkDensity.Sparse => 0.05f, 
			LandmarkDensity.SlightlyMoreSparse => 0.33f, 
			LandmarkDensity.SlightlySparse => 0.66f, 
			LandmarkDensity.Normal => 1f, 
			LandmarkDensity.SlightlyCrowded => 1.5f, 
			LandmarkDensity.SlightlyMoreCrowded => 2f, 
			LandmarkDensity.Crowded => 3f, 
			_ => 1f, 
		};
	}
}
