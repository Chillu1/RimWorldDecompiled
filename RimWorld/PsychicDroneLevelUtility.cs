using Verse;

namespace RimWorld;

public static class PsychicDroneLevelUtility
{
	public static string GetLabel(this PsychicDroneLevel level)
	{
		return level switch
		{
			PsychicDroneLevel.None => "PsychicDroneLevel_None".Translate(), 
			PsychicDroneLevel.GoodMedium => "PsychicDroneLevel_GoodMedium".Translate(), 
			PsychicDroneLevel.BadLow => "PsychicDroneLevel_BadLow".Translate(), 
			PsychicDroneLevel.BadMedium => "PsychicDroneLevel_BadMedium".Translate(), 
			PsychicDroneLevel.BadHigh => "PsychicDroneLevel_BadHigh".Translate(), 
			PsychicDroneLevel.BadExtreme => "PsychicDroneLevel_BadExtreme".Translate(), 
			_ => "error", 
		};
	}

	public static string GetLabelCap(this PsychicDroneLevel level)
	{
		return level.GetLabel().CapitalizeFirst();
	}
}
