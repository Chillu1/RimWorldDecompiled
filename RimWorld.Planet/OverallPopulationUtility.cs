using System;

namespace RimWorld.Planet
{
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
			switch (population)
			{
			case OverallPopulation.AlmostNone:
				return 0.1f;
			case OverallPopulation.Little:
				return 0.4f;
			case OverallPopulation.LittleBitLess:
				return 0.7f;
			case OverallPopulation.Normal:
				return 1f;
			case OverallPopulation.LittleBitMore:
				return 1.5f;
			case OverallPopulation.High:
				return 2f;
			case OverallPopulation.VeryHigh:
				return 2.75f;
			default:
				return 1f;
			}
		}
	}
}
