using Verse;

namespace RimWorld;

public static class HateChantDroneLevelUtility
{
	public static string GetLabel(this HateChantDroneLevel level)
	{
		return level switch
		{
			HateChantDroneLevel.None => "HateChantDroneLevel_None".Translate(), 
			HateChantDroneLevel.VeryLow => "HateChantDroneLevel_VeryLow".Translate(), 
			HateChantDroneLevel.Low => "HateChantDroneLevel_Low".Translate(), 
			HateChantDroneLevel.Medium => "HateChantDroneLevel_Medium".Translate(), 
			HateChantDroneLevel.High => "HateChantDroneLevel_High".Translate(), 
			HateChantDroneLevel.Extreme => "HateChantDroneLevel_Extreme".Translate(), 
			_ => "error", 
		};
	}

	public static string GetLabelCap(this HateChantDroneLevel level)
	{
		return level.GetLabel().CapitalizeFirst();
	}
}
