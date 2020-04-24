using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse.Sound
{
	public abstract class Sample
	{
		public SubSoundDef subDef;

		public AudioSource source;

		public float startRealTime;

		public int startTick;

		public float resolvedVolume;

		public float resolvedPitch;

		private bool mappingsApplied;

		private Dictionary<SoundParamTarget, float> volumeInMappings = new Dictionary<SoundParamTarget, float>();

		public float AgeRealTime => Time.realtimeSinceStartup - startRealTime;

		public int AgeTicks
		{
			get
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					return Find.TickManager.TicksGame - startTick;
				}
				return (int)(AgeRealTime * 60f);
			}
		}

		public abstract float ParentStartRealTime
		{
			get;
		}

		public abstract float ParentStartTick
		{
			get;
		}

		public abstract float ParentHashCode
		{
			get;
		}

		public abstract SoundParams ExternalParams
		{
			get;
		}

		public abstract SoundInfo Info
		{
			get;
		}

		public Map Map => Info.Maker.Map;

		protected bool TestPlaying => Info.testPlay;

		protected float MappedVolumeMultiplier
		{
			get
			{
				float num = 1f;
				foreach (float value in volumeInMappings.Values)
				{
					num *= value;
				}
				return num;
			}
		}

		protected float ContextVolumeMultiplier
		{
			get
			{
				if (SoundDefHelper.CorrectContextNow(subDef.parentDef, Map))
				{
					return 1f;
				}
				return 0f;
			}
		}

		protected virtual float Volume
		{
			get
			{
				if (subDef.muteWhenPaused && Current.ProgramState == ProgramState.Playing && Find.TickManager.Paused && !TestPlaying)
				{
					return 0f;
				}
				return resolvedVolume * Info.volumeFactor * MappedVolumeMultiplier * ContextVolumeMultiplier;
			}
		}

		public float SanitizedVolume => AudioSourceUtility.GetSanitizedVolume(Volume, subDef.parentDef);

		protected virtual float Pitch
		{
			get
			{
				float num = resolvedPitch * Info.pitchFactor;
				if (subDef.tempoAffectedByGameSpeed && Current.ProgramState == ProgramState.Playing && !TestPlaying && !Find.TickManager.Paused)
				{
					num *= Find.TickManager.TickRateMultiplier;
				}
				return num;
			}
		}

		public float SanitizedPitch => AudioSourceUtility.GetSanitizedPitch(Pitch, subDef.parentDef);

		public Sample(SubSoundDef def)
		{
			subDef = def;
			resolvedVolume = def.RandomizedVolume();
			resolvedPitch = def.pitchRange.RandomInRange;
			startRealTime = Time.realtimeSinceStartup;
			if (Current.ProgramState == ProgramState.Playing)
			{
				startTick = Find.TickManager.TicksGame;
			}
			else
			{
				startTick = 0;
			}
			foreach (SoundParamTarget_Volume item in subDef.paramMappings.Select((SoundParameterMapping m) => m.outParam).OfType<SoundParamTarget_Volume>())
			{
				volumeInMappings.Add(item, 0f);
			}
		}

		public virtual void Update()
		{
			source.pitch = SanitizedPitch;
			ApplyMappedParameters();
			source.volume = SanitizedVolume;
			if (source.volume < 0.001f)
			{
				source.mute = true;
			}
			else
			{
				source.mute = false;
			}
			if (!subDef.tempoAffectedByGameSpeed || TestPlaying)
			{
				return;
			}
			if (Current.ProgramState == ProgramState.Playing && Find.TickManager.Paused)
			{
				if (source.isPlaying)
				{
					source.Pause();
				}
			}
			else if (!source.isPlaying)
			{
				source.UnPause();
			}
		}

		public void ApplyMappedParameters()
		{
			for (int i = 0; i < subDef.paramMappings.Count; i++)
			{
				SoundParameterMapping soundParameterMapping = subDef.paramMappings[i];
				if (soundParameterMapping.paramUpdateMode != SoundParamUpdateMode.OncePerSample || !mappingsApplied)
				{
					soundParameterMapping.Apply(this);
				}
			}
			mappingsApplied = true;
		}

		public void SignalMappedVolume(float value, SoundParamTarget sourceParam)
		{
			volumeInMappings[sourceParam] = value;
		}

		public virtual void SampleCleanup()
		{
			for (int i = 0; i < subDef.paramMappings.Count; i++)
			{
				SoundParameterMapping soundParameterMapping = subDef.paramMappings[i];
				if (soundParameterMapping.curve.HasView)
				{
					soundParameterMapping.curve.View.ClearDebugInputFrom(this);
				}
			}
		}

		public override string ToString()
		{
			return "Sample_" + subDef.name + " volume=" + source.volume + " at " + source.transform.position.ToIntVec3();
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(startRealTime.GetHashCode(), subDef);
		}
	}
}
