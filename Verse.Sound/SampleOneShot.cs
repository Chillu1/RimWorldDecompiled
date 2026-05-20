using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound;

public class SampleOneShot : Sample
{
	public SoundInfo info;

	private const float MaxVacuumVolumePercent = 0.05f;

	private SoundParams externalParams = new SoundParams();

	private static readonly SimpleCurve VacuumFrequencyCutoffCurve = new SimpleCurve
	{
		new CurvePoint(0f, 22000f),
		new CurvePoint(0.25f, 4000f),
		new CurvePoint(1f, 400f)
	};

	private static readonly SoundFilterLowPass VacuumHighPassFilter = new SoundFilterLowPass
	{
		lowpassResonaceQ = 0f
	};

	public override float ParentStartRealTime => startRealTime;

	public override float ParentStartTick => startTick;

	public override float ParentHashCode => GetHashCode();

	public override SoundParams ExternalParams => externalParams;

	public override SoundInfo Info => info;

	private SampleOneShot(SubSoundDef def)
		: base(def)
	{
	}

	public static SampleOneShot TryMakeAndPlay(SubSoundDef def, AudioClip clip, SoundInfo info)
	{
		if ((double)info.pitchFactor <= 0.0001)
		{
			Log.ErrorOnce($"Played sound with pitchFactor {info.pitchFactor}: {def}, {info}", 632321);
			return null;
		}
		SampleOneShot sampleOneShot = new SampleOneShot(def)
		{
			info = info,
			source = Find.SoundRoot.sourcePool.GetSource(def.onCamera)
		};
		if (sampleOneShot.source == null)
		{
			return null;
		}
		sampleOneShot.source.clip = clip;
		sampleOneShot.source.volume = sampleOneShot.SanitizedVolume;
		sampleOneShot.source.pitch = sampleOneShot.SanitizedPitch;
		sampleOneShot.source.minDistance = sampleOneShot.subDef.distRange.TrueMin;
		sampleOneShot.source.maxDistance = sampleOneShot.subDef.distRange.TrueMax;
		if (!def.onCamera)
		{
			sampleOneShot.source.gameObject.transform.position = info.Maker.Cell.ToVector3ShiftedWithAltitude(0f);
			sampleOneShot.source.minDistance = def.distRange.TrueMin;
			sampleOneShot.source.maxDistance = def.distRange.TrueMax;
			sampleOneShot.source.spatialBlend = 1f;
			if (def.canVacuumDampen)
			{
				Map map = info.Maker.Map;
				if (map != null && map.Biome?.inVacuum == true)
				{
					float num = EasingFunctions.EaseOutCubic(info.Maker.Cell.GetVacuum(info.Maker.Map));
					sampleOneShot.source.volume *= Mathf.Lerp(1f, 0.05f, num);
					VacuumHighPassFilter.cutoffFrequency = VacuumFrequencyCutoffCurve.Evaluate(num);
					VacuumHighPassFilter.SetupOn(sampleOneShot.source);
				}
			}
		}
		else
		{
			sampleOneShot.source.spatialBlend = 0f;
		}
		for (int i = 0; i < def.filters.Count; i++)
		{
			def.filters[i].SetupOn(sampleOneShot.source);
		}
		foreach (KeyValuePair<string, float> definedParameter in info.DefinedParameters)
		{
			sampleOneShot.externalParams[definedParameter.Key] = definedParameter.Value;
		}
		sampleOneShot.Update();
		sampleOneShot.source.Play();
		Find.SoundRoot.oneShotManager.TryAddPlayingOneShot(sampleOneShot);
		return sampleOneShot;
	}
}
