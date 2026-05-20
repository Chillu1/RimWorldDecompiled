using Verse;

namespace RimWorld;

public class CompRitualTargetEffecterSpawner : CompRitualEffectSpawner
{
	private Effecter effecter;

	private CompProperties_RitualTargetEffecterSpawner Props => (CompProperties_RitualTargetEffecterSpawner)props;

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
		effecter?.Cleanup();
		effecter = null;
	}

	protected override void Tick_InRitual(LordJob_Ritual ritual)
	{
	}

	protected override void Tick_InRitualInterval(LordJob_Ritual ritual)
	{
		if (!(Props.minRitualProgress > 0f) || !(ritual.Progress < Props.minRitualProgress))
		{
			if (effecter == null)
			{
				effecter = Props.effecter.Spawn();
				effecter.Trigger(parent, parent);
			}
			effecter.EffectTick(parent, parent);
		}
	}

	protected override void Tick_OutOfRitualInterval()
	{
		effecter?.Cleanup();
		effecter = null;
	}
}
