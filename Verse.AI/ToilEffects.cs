using System;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse.AI;

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

	public static Toil PlaySustainerOrSound(this Toil toil, SoundDef soundDef, float pitchFactor = 1f)
	{
		return toil.PlaySustainerOrSound(() => soundDef, pitchFactor);
	}

	public static Toil PlaySustainerOrSound(this Toil toil, Func<SoundDef> soundDefGetter, float pitchFactor = 1f)
	{
		Sustainer sustainer = null;
		toil.AddPreInitAction(delegate
		{
			SoundDef soundDef = soundDefGetter();
			if (soundDef != null && !soundDef.sustain)
			{
				soundDef.PlayOneShot(new TargetInfo(toil.GetActor().Position, toil.GetActor().Map));
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
					info.pitchFactor = pitchFactor;
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

	public static Toil WithEffect(this Toil toil, EffecterDef effectDef, TargetIndex ind, Color? overrideColor = null)
	{
		return toil.WithEffect(() => effectDef, ind, overrideColor);
	}

	public static Toil WithEffect(this Toil toil, EffecterDef effectDef, Func<LocalTargetInfo> effectTargetGetter, Color? overrideColor = null)
	{
		return toil.WithEffect(() => effectDef, effectTargetGetter, overrideColor);
	}

	public static Toil WithEffect(this Toil toil, Func<EffecterDef> effecterDefGetter, TargetIndex ind, Color? overrideColor = null)
	{
		return toil.WithEffect(effecterDefGetter, () => toil.actor.CurJob.GetTarget(ind), overrideColor);
	}

	public static Toil WithEffect(this Toil toil, Func<EffecterDef> effecterDefGetter, Thing thing, Color? overrideColor = null)
	{
		return toil.WithEffect(effecterDefGetter, () => thing, overrideColor);
	}

	public static Toil WithEffect(this Toil toil, Func<EffecterDef> effecterDefGetter, Func<LocalTargetInfo> effectTargetGetter, Color? overrideColor = null)
	{
		Effecter effecter = null;
		toil.AddPreTickAction(delegate
		{
			if (effecter == null)
			{
				EffecterDef effecterDef = effecterDefGetter();
				if (effecterDef != null)
				{
					toil.actor.rotationTracker.FaceTarget(effectTargetGetter());
					effecter = effecterDef.Spawn();
					effecter.Trigger(toil.actor, effectTargetGetter().ToTargetInfo(toil.actor.Map));
					if (overrideColor.HasValue)
					{
						foreach (SubEffecter child in effecter.children)
						{
							if (child is SubEffecter_Sprayer subEffecter_Sprayer)
							{
								subEffecter_Sprayer.colorOverride = overrideColor;
							}
						}
					}
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

	public static Toil WithProgressBar(this Toil toil, TargetIndex ind, Func<float> progressGetter, bool interpolateBetweenActorAndTarget = false, float offsetZ = -0.5f, bool alwaysShow = false)
	{
		Effecter effecter = null;
		toil.AddPreTickIntervalAction(delegate
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
					LocalTargetInfo localTargetInfo = ((ind == TargetIndex.None) ? LocalTargetInfo.Invalid : toil.actor.CurJob.GetTarget(ind));
					if (!localTargetInfo.IsValid || (localTargetInfo.HasThing && !localTargetInfo.Thing.Spawned))
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
						mote.alwaysShow = alwaysShow;
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
