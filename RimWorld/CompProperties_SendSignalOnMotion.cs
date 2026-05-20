using Verse;

namespace RimWorld;

public class CompProperties_SendSignalOnMotion : CompProperties
{
	public bool triggerOnPawnInRoom;

	public float radius;

	public int enableAfterTicks;

	public bool onlyHumanlike;

	public bool triggeredBySkipPsycasts;

	[NoTranslate]
	public string signalTag;

	public CompProperties_SendSignalOnMotion()
	{
		compClass = typeof(CompSendSignalOnMotion);
	}
}
