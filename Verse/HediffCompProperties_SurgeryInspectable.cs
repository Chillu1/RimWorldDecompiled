namespace Verse;

public class HediffCompProperties_SurgeryInspectable : HediffCompProperties
{
	[MustTranslate]
	public string surgicalDetectionDesc;

	public bool preventLetterIfPreviouslyDetected;

	public HediffCompProperties_SurgeryInspectable()
	{
		compClass = typeof(HediffComp_SurgeryInspectable);
	}
}
