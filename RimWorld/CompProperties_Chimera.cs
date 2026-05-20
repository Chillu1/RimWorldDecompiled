using Verse;

namespace RimWorld;

public class CompProperties_Chimera : CompProperties
{
	public float rageEndHealthPercentThreshold = 0.98f;

	[MustTranslate]
	public string simpleAnimalLabel;

	public CompProperties_Chimera()
	{
		compClass = typeof(CompChimera);
	}
}
