using Verse;

namespace RimWorld;

public class CompProperties_Countdown : CompProperties
{
	[MustTranslate]
	public string label;

	public CompProperties_Countdown()
	{
		compClass = typeof(CompCountdown);
	}
}
