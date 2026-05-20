using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Sound;

public class SampleOneShotManager
{
	private List<SampleOneShot> samples = new List<SampleOneShot>();

	private List<SampleOneShot> cleanupList = new List<SampleOneShot>();

	public IEnumerable<SampleOneShot> PlayingOneShots => samples;

	private float CameraDistanceSquaredOf(SoundInfo info)
	{
		return (Find.CameraDriver.MapPosition - info.Maker.Cell).LengthHorizontalSquared;
	}

	private float ImportanceOf(SampleOneShot sample)
	{
		return ImportanceOf(sample.subDef.parentDef, sample.info, sample.AgeRealTime);
	}

	private float ImportanceOf(SoundDef def, SoundInfo info, float ageRealTime)
	{
		if (def.priorityMode == VoicePriorityMode.PrioritizeNearest)
		{
			return 1f / (CameraDistanceSquaredOf(info) + 1f);
		}
		if (def.priorityMode == VoicePriorityMode.PrioritizeNewest)
		{
			return 1f / (ageRealTime + 1f);
		}
		if (def.priorityMode == VoicePriorityMode.PrioritizeExisting)
		{
			return ageRealTime;
		}
		throw new NotImplementedException();
	}

	public bool CanAddPlayingOneShot(SoundDef def, SoundInfo info)
	{
		if (!SoundDefHelper.CorrectContextNow(def, info.Maker.Map))
		{
			return false;
		}
		if (samples.Where((SampleOneShot s) => s.subDef.parentDef == def && s.AgeRealTime < 0.05f).Count() >= def.MaxSimultaneousSamples)
		{
			return false;
		}
		return true;
	}

	public void TryAddPlayingOneShot(SampleOneShot newSample)
	{
		if (samples.Where((SampleOneShot s) => s.subDef.IsSameOrHasSameTag(newSample.subDef)).Count() >= newSample.subDef.parentDef.maxVoices)
		{
			SampleOneShot sampleOneShot = LeastImportantOf(newSample.subDef);
			sampleOneShot.source.Stop();
			samples.Remove(sampleOneShot);
		}
		samples.Add(newSample);
	}

	private SampleOneShot LeastImportantOf(SubSoundDef def)
	{
		SampleOneShot sampleOneShot = null;
		for (int i = 0; i < samples.Count; i++)
		{
			SampleOneShot sampleOneShot2 = samples[i];
			if (sampleOneShot2.subDef.IsSameOrHasSameTag(def) && (sampleOneShot == null || ImportanceOf(sampleOneShot2) < ImportanceOf(sampleOneShot)))
			{
				sampleOneShot = sampleOneShot2;
			}
		}
		return sampleOneShot;
	}

	private SampleOneShot LeastImportantOf(SoundDef def)
	{
		SampleOneShot sampleOneShot = null;
		for (int i = 0; i < samples.Count; i++)
		{
			SampleOneShot sampleOneShot2 = samples[i];
			if (sampleOneShot2.subDef.parentDef == def && (sampleOneShot == null || ImportanceOf(sampleOneShot2) < ImportanceOf(sampleOneShot)))
			{
				sampleOneShot = sampleOneShot2;
			}
		}
		return sampleOneShot;
	}

	public void SampleOneShotManagerUpdate()
	{
		for (int i = 0; i < samples.Count; i++)
		{
			samples[i].Update();
		}
		cleanupList.Clear();
		for (int j = 0; j < samples.Count; j++)
		{
			SampleOneShot sampleOneShot = samples[j];
			if (sampleOneShot.source == null || (!sampleOneShot.source.isPlaying && (!sampleOneShot.subDef.tempoAffectedByGameSpeed || !Find.TickManager.Paused)) || !SoundDefHelper.CorrectContextNow(sampleOneShot.subDef.parentDef, sampleOneShot.Map))
			{
				if (sampleOneShot.source != null && sampleOneShot.source.isPlaying)
				{
					sampleOneShot.source.Stop();
				}
				sampleOneShot.SampleCleanup();
				cleanupList.Add(sampleOneShot);
			}
		}
		if (cleanupList.Count > 0)
		{
			samples.RemoveAll((SampleOneShot s) => cleanupList.Contains(s));
		}
	}
}
