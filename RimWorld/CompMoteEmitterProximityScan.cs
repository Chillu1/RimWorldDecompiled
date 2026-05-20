using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompMoteEmitterProximityScan : CompMoteEmitter
{
	private CompSendSignalOnMotion proximityCompCached;

	private Sustainer sustainer;

	private CompProperties_MoteEmitterProximityScan Props => (CompProperties_MoteEmitterProximityScan)props;

	private CompSendSignalOnMotion ProximityComp => proximityCompCached ?? (proximityCompCached = parent.GetComp<CompSendSignalOnMotion>());

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (sustainer != null && !sustainer.Ended)
		{
			sustainer.End();
		}
	}

	public override void CompTick()
	{
		if (!parent.Spawned)
		{
			return;
		}
		if (ProximityComp == null || ProximityComp.Sent)
		{
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.End();
			}
			return;
		}
		if (mote == null)
		{
			Emit();
		}
		if (!Props.soundEmitting.NullOrUndefined())
		{
			if (sustainer == null || sustainer.Ended)
			{
				sustainer = Props.soundEmitting.TrySpawnSustainer(SoundInfo.InMap(parent));
			}
			sustainer.Maintain();
		}
		if (mote == null)
		{
			return;
		}
		mote.Maintain();
		float a;
		if (!ProximityComp.Enabled)
		{
			if (ticksSinceLastEmitted >= Props.emissionInterval)
			{
				ticksSinceLastEmitted = 0;
			}
			else
			{
				ticksSinceLastEmitted++;
			}
			float num = (float)ticksSinceLastEmitted / 60f;
			a = ((num <= Props.warmupPulseFadeInTime) ? ((!(Props.warmupPulseFadeInTime > 0f)) ? 1f : (num / Props.warmupPulseFadeInTime)) : ((num <= Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime) ? 1f : ((!(Props.warmupPulseFadeOutTime > 0f)) ? 1f : (1f - Mathf.InverseLerp(Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime, Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime + Props.warmupPulseFadeOutTime, num)))));
		}
		else
		{
			a = 1f;
		}
		mote.instanceColor.a = a;
	}
}
