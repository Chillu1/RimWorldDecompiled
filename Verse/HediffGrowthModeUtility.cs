using System;

namespace Verse
{
	public static class HediffGrowthModeUtility
	{
		public static string GetLabel(this HediffGrowthMode m)
		{
			switch (m)
			{
			case HediffGrowthMode.Growing:
				return "HediffGrowthMode_Growing".Translate();
			case HediffGrowthMode.Stable:
				return "HediffGrowthMode_Stable".Translate();
			case HediffGrowthMode.Remission:
				return "HediffGrowthMode_Remission".Translate();
			default:
				throw new ArgumentException();
			}
		}
	}
}
