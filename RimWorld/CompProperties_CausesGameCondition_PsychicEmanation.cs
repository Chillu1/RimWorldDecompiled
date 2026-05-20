namespace RimWorld;

public class CompProperties_CausesGameCondition_PsychicEmanation : CompProperties_CausesGameCondition
{
	public PsychicDroneLevel droneLevel = PsychicDroneLevel.BadMedium;

	public int droneLevelIncreaseInterval = int.MinValue;

	public CompProperties_CausesGameCondition_PsychicEmanation()
	{
		compClass = typeof(CompCauseGameCondition_PsychicEmanation);
	}
}
