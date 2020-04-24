using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse.AI
{
	public static class ToilEffects
	{
		public static Toil PlaySoundAtStart(this Toil toil, SoundDef sound)
		{
			toil.AddPreInitAction(delegate
			{
				sound.PlayOneShot(new TargetInfo(toil.GetActor().Position, toil.GetActor().Map));
			});
			return toil;
		}

		public static Toil PlaySoundAtEnd(this Toil toil, SoundDef sound)
		{
			toil.AddFinishAction(delegate
			{
				sound.PlayOneShot(new TargetInfo(toil.GetActor().Position, toil.GetActor().Map));
			});
			return toil;
		}

		public static Toil PlaySustainerOrSound(this Toil toil, SoundDef soundDef)
		{
			return toil.PlaySustainerOrSound(() => soundDef);
		}

		public static Toil PlaySustainerOrSound(this Toil toil, Func<SoundDef> soundDefGetter)
		{
			Sustainer sustainer = null;
			toil.AddPreInitAction(delegate
			{
				SoundDef soundDef2 = soundDefGetter();
				if (soundDef2 != null && !soundDef2.sustain)
				{
					soundDef2.PlayOneShot(new TargetInfo(toil.GetActor().Position, toil.GetActor().Map));
				}
			});
			toil.AddPreTickAction(delegate
			{
				if (sustainer == null || sustainer.Ended)
				{
					SoundDef soundDef = soundDefGetter();
					if (soundDef != null && soundDef.sustain)
					{
						SoundInfo info = SoundInfo.InMap(toil.actor, MaintenanceType.PerTick);
						sustainer = soundDef.TrySpawnSustainer(info);
					}
				}
				else
				{
					sustainer.Maintain();
				}
			});
			return toil;
		}

		public static Toil WithEffect(this Toil toil, EffecterDef effectDef, TargetIndex ind)
		{
			return toil.WithEffect(() => effectDef, ind);
		}

		public static Toil WithEffect(this Toil toil, Func<EffecterDef> effecterDefGetter, TargetIndex ind)
		{
			return toil.WithEffect(effecterDefGetter, () => toil.actor.CurJob.GetTarget(ind));
		}

		public static Toil WithEffect(this Toil toil, Func<EffecterDef> effecterDefGetter, Thing thing)
		{
			return toil.WithEffect(effecterDefGetter, () => thing);
		}

		public static Toil WithEffect(this Toil toil, Func<EffecterDef> effecterDefGetter, Func<LocalTargetInfo> effectTargetGetter)
		{
			Effecter effecter = null;
			toil.AddPreTickAction(delegate
			{
				if (effecter == null)
				{
					EffecterDef effecterDef = effecterDefGetter();
					if (effecterDef != null)
					{
						effecter = effecterDef.Spawn();
					}
				}
				else
				{
					effecter.EffectTick(toil.actor, effectTargetGetter().ToTargetInfo(toil.actor.Map));
				}
			});
			toil.AddFinishAction(delegate
			{
				if (effecter != null)
				{
					effecter.Cleanup();
					effecter = null;
				}
			});
			return toil;
		}

		public static Toil WithProgressBar(this Toil toil, TargetIndex ind, Func<float> progressGetter, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f)
		{
			Effecter effecter = null;
			toil.AddPreTickAction(delegate
			{
				if (toil.actor.Faction == Faction.OfPlayer)
				{
					if (effecter == null)
					{
						EffecterDef progressBar = EffecterDefOf.ProgressBar;
						effecter = progressBar.Spawn();
					}
					else
					{
						LocalTargetInfo target = toil.actor.CurJob.GetTarget(ind);
						if (!target.IsValid || (target.HasThing && !target.Thing.Spawned))
						{
							effecter.EffectTick(toil.actor, TargetInfo.Invalid);
						}
						else if (interpolateBetweenActorAndTarget)
						{
							effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map), toil.actor);
						}
						else
						{
							effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map), TargetInfo.Invalid);
						}
						MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
						if (mote != null)
						{
							mote.progress = Mathf.Clamp01(progressGetter());
							mote.offsetZ = offsetZ;
						}
					}
				}
			});
			toil.AddFinishAction(delegate
			{
				if (effecter != null)
				{
					effecter.Cleanup();
					effecter = null;
				}
			});
			return toil;
		}

		public static Toil WithProgressBarToilDelay(this Toil toil, TargetIndex ind, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f)
		{
			return toil.WithProgressBar(ind, () => 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)toil.defaultDuration, interpolateBetweenActorAndTarget, offsetZ);
		}

		public static Toil WithProgressBarToilDelay(this Toil toil, TargetIndex ind, int toilDuration, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f)
		{
			return toil.WithProgressBar(ind, () => 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)toilDuration, interpolateBetweenActorAndTarget, offsetZ);
		}
	}
}
