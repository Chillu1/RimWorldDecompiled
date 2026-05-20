using Verse;

namespace RimWorld;

public class CompProperties_EffecterOnDeath : CompProperties
{
	public EffecterDef effecterDef;

	public CompProperties_EffecterOnDeath()
	{
		compClass = typeof(EffecterOnDeath);
	}
}
