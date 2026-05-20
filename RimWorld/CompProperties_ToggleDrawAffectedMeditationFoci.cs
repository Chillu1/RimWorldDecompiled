using Verse;

namespace RimWorld;

public class CompProperties_ToggleDrawAffectedMeditationFoci : CompProperties
{
	public bool defaultEnabled = true;

	public CompProperties_ToggleDrawAffectedMeditationFoci()
	{
		compClass = typeof(CompToggleDrawAffectedMeditationFoci);
	}
}
