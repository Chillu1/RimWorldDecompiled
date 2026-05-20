using Verse;

namespace RimWorld;

public class CompProperties_FadesInOut : CompProperties
{
	public float fadeInSecs;

	public float fadeOutSecs;

	public float solidTimeSecs;

	public CompProperties_FadesInOut()
	{
		compClass = typeof(CompFadesInOut);
	}
}
