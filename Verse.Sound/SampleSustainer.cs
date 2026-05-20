using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound;

public class SampleSustainer : Sample
{
	public SubSustainer subSustainer;

	public float scheduledEndTime;

	public bool resolvedSkipAttack;

	public override float ParentStartRealTime => subSustainer.creationRealTime;

	public override float ParentStartTick => subSustainer.creationTick;

	public override float ParentHashCode => subSustainer.GetHashCode();

	public override SoundParams ExternalParams => subSustainer.ExternalParams;

	public override SoundInfo Info => subSustainer.Info;

	protected override float Volume
	{
		get
		{
			float num = base.Volume * subSustainer.parent.scopeFader.inScopePercent;
			float num2 = 1f;
			if (subSustainer.parent.Ended)
			{
				num2 = 1f - Mathf.Min(subSustainer.parent.TimeSinceEnd / subDef.parentDef.sustainFadeoutTime, 1f);
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (base.AgeRealTime < subDef.sustainAttack)
			{
				if (resolvedSkipAttack || subDef.sustainAttack < 0.01f)
				{
					return num * num2;
				}
				float f = base.AgeRealTime / subDef.sustainAttack;
				f = Mathf.Sqrt(f);
				return Mathf.Lerp(0f, num, f) * num2;
			}
			if (realtimeSinceStartup > scheduledEndTime - subDef.sustainRelease)
			{
				float num3 = (realtimeSinceStartup - (scheduledEndTime - subDef.sustainRelease)) / subDef.sustainRelease;
				num3 = 1f - num3;
				num3 = Mathf.Max(num3, 0f);
				num3 = Mathf.Sqrt(num3);
				num3 = 1f - num3;
				return Mathf.Lerp(num, 0f, num3) * num2;
			}
			return num * num2;
		}
	}

	private SampleSustainer(SubSoundDef def)
		: base(def)
	{
	}

	public static SampleSustainer TryMakeAndPlay(SubSustainer subSus, AudioClip clip, float scheduledEndTime, float startTime = 0f)
	{
		SampleSustainer sampleSustainer = new SampleSustainer(subSus.subDef);
		sampleSustainer.subSustainer = subSus;
		sampleSustainer.scheduledEndTime = scheduledEndTime;
		GameObject gameObject = new GameObject("SampleSource_" + sampleSustainer.subDef.name + "_" + sampleSustainer.startRealTime);
		GameObject gameObject2 = (subSus.subDef.onCamera ? Find.Camera.gameObject : subSus.parent.worldRootObject);
		gameObject.transform.parent = gameObject2.transform;
		gameObject.transform.localPosition = Vector3.zero;
		sampleSustainer.source = AudioSourceMaker.NewAudioSourceOn(gameObject);
		if (sampleSustainer.source == null)
		{
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			return null;
		}
		sampleSustainer.source.clip = clip;
		sampleSustainer.source.volume = sampleSustainer.SanitizedVolume;
		sampleSustainer.source.pitch = sampleSustainer.SanitizedPitch;
		sampleSustainer.source.minDistance = sampleSustainer.subDef.distRange.TrueMin;
		sampleSustainer.source.maxDistance = sampleSustainer.subDef.distRange.TrueMax;
		sampleSustainer.source.spatialBlend = 1f;
		List<SoundFilter> filters = sampleSustainer.subDef.filters;
		for (int i = 0; i < filters.Count; i++)
		{
			filters[i].SetupOn(sampleSustainer.source);
		}
		if (sampleSustainer.subDef.sustainLoop)
		{
			sampleSustainer.source.loop = true;
		}
		sampleSustainer.Update();
		sampleSustainer.source.time = startTime;
		sampleSustainer.source.Play();
		sampleSustainer.source.Play();
		return sampleSustainer;
	}

	public override void SampleCleanup()
	{
		base.SampleCleanup();
		if (source != null && source.gameObject != null)
		{
			Object.Destroy(source.gameObject);
		}
	}
}
