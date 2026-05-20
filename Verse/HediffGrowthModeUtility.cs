using System;

namespace Verse;

public static class HediffGrowthModeUtility
{
	public static string GetLabel(this HediffGrowthMode m)
	{
		return m switch
		{
			HediffGrowthMode.Growing => "HediffGrowthMode_Growing".Translate(), 
			HediffGrowthMode.Stable => "HediffGrowthMode_Stable".Translate(), 
			HediffGrowthMode.Remission => "HediffGrowthMode_Remission".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}
}
