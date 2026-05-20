using Verse;

namespace RimWorld;

public class CompProperties_UseEffect : CompProperties
{
	public bool doCameraShake;

	public ThingDef moteOnUsed;

	public float moteOnUsedScale = 1f;

	public FleckDef fleckOnUsed;

	public float fleckOnUsedScale = 1f;

	public EffecterDef effecterOnUsed;

	public EffecterDef warmupEffecter;

	public CompProperties_UseEffect()
	{
		compClass = typeof(CompUseEffect);
	}
}
