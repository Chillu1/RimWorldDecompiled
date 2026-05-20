using Verse;

namespace RimWorld;

public class CompProperties_SendSignalOnCountdown : CompProperties
{
	public SimpleCurve countdownCurveTicks;

	[NoTranslate]
	public string signalTag;

	public CompProperties_SendSignalOnCountdown()
	{
		compClass = typeof(CompSendSignalOnCountdown);
	}
}
