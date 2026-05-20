using Verse;

namespace RimWorld;

public class CompProperties_UseEffectPlayWarmupSound : CompProperties_UseEffect
{
	public SoundDef warmupSound;

	public CompProperties_UseEffectPlayWarmupSound()
	{
		compClass = typeof(CompUseEffect_PlayWarmupSound);
	}
}
