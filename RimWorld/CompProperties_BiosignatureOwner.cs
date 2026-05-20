using Verse;

namespace RimWorld;

public class CompProperties_BiosignatureOwner : CompProperties
{
	public bool requiresAnalysis = true;

	public CompProperties_BiosignatureOwner()
	{
		compClass = typeof(CompBiosignatureOwner);
	}
}
