using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompMoteEmitterProximityScan : CompMoteEmitter
	{
		private CompSendSignalOnPawnProximity proximityCompCached;

		private Sustainer sustainer;

		private CompProperties_MoteEmitterProximityScan Props => (CompProperties_MoteEmitterProximityScan)props;

		private CompSendSignalOnPawnProximity ProximityComp => proximityCompCached ?? (proximityCompCached = parent.GetComp<CompSendSignalOnPawnProximity>());

		public override void PostDeSpawn(Map map)
		{
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.End();
			}
		}

		public override void CompTick()
		{
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
			mote?.Maintain();
			float num = 1f;
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
				float num2 = (float)ticksSinceLastEmitted / 60f;
				num = ((num2 <= Props.warmupPulseFadeInTime) ? ((!(Props.warmupPulseFadeInTime > 0f)) ? 1f : (num2 / Props.warmupPulseFadeInTime)) : ((num2 <= Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime) ? 1f : ((!(Props.warmupPulseFadeOutTime > 0f)) ? 1f : (1f - Mathf.InverseLerp(Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime, Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime + Props.warmupPulseFadeOutTime, num2)))));
			}
			else
			{
				num = 1f;
			}
			mote.instanceColor.a = num;
		}
	}
}
