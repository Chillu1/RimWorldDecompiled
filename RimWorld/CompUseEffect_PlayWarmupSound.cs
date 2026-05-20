using Verse.Sound;

namespace RimWorld;

public class CompUseEffect_PlayWarmupSound : CompUseEffect
{
	private Sustainer sustainer;

	private CompProperties_UseEffectPlayWarmupSound Props => (CompProperties_UseEffectPlayWarmupSound)props;

	public override void PrepareTick()
	{
		if (Props.warmupSound != null && sustainer == null)
		{
			SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
			sustainer = Props.warmupSound.TrySpawnSustainer(info);
		}
		if (sustainer != null && !sustainer.Ended)
		{
			sustainer.Maintain();
		}
	}
}
