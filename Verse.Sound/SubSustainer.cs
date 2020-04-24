using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse.Sound
{
	public class SubSustainer
	{
		public Sustainer parent;

		public SubSoundDef subDef;

		private List<SampleSustainer> samples = new List<SampleSustainer>();

		private float nextSampleStartTime;

		public int creationFrame = -1;

		public int creationTick = -1;

		public float creationRealTime = -1f;

		private const float MinSampleStartInterval = 0.01f;

		public SoundInfo Info => parent.info;

		public SoundParams ExternalParams => parent.externalParams;

		public SubSustainer(Sustainer parent, SubSoundDef subSoundDef)
		{
			this.parent = parent;
			subDef = subSoundDef;
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				creationFrame = Time.frameCount;
				creationRealTime = Time.realtimeSinceStartup;
				if (Current.ProgramState == ProgramState.Playing)
				{
					creationTick = Find.TickManager.TicksGame;
				}
				if (subDef.startDelayRange.TrueMax < 0.001f)
				{
					StartSample();
				}
				else
				{
					nextSampleStartTime = Time.realtimeSinceStartup + subDef.startDelayRange.RandomInRange;
				}
			});
		}

		private void StartSample()
		{
			ResolvedGrain resolvedGrain = subDef.RandomizedResolvedGrain();
			if (resolvedGrain == null)
			{
				Log.Error("SubSustainer for " + subDef + " of " + parent.def + " could not resolve any grains.");
				parent.End();
				return;
			}
			float num = (!subDef.sustainLoop) ? resolvedGrain.duration : subDef.sustainLoopDurationRange.RandomInRange;
			float num2 = Time.realtimeSinceStartup + num;
			nextSampleStartTime = num2 + subDef.sustainIntervalRange.RandomInRange;
			if (nextSampleStartTime < Time.realtimeSinceStartup + 0.01f)
			{
				nextSampleStartTime = Time.realtimeSinceStartup + 0.01f;
			}
			if (resolvedGrain is ResolvedGrain_Silence)
			{
				return;
			}
			SampleSustainer sampleSustainer = SampleSustainer.TryMakeAndPlay(this, ((ResolvedGrain_Clip)resolvedGrain).clip, num2);
			if (sampleSustainer != null)
			{
				if (subDef.sustainSkipFirstAttack && Time.frameCount == creationFrame)
				{
					sampleSustainer.resolvedSkipAttack = true;
				}
				samples.Add(sampleSustainer);
			}
		}

		public void SubSustainerUpdate()
		{
			for (int num = samples.Count - 1; num >= 0; num--)
			{
				if (Time.realtimeSinceStartup > samples[num].scheduledEndTime)
				{
					EndSample(samples[num]);
				}
			}
			if (Time.realtimeSinceStartup > nextSampleStartTime)
			{
				StartSample();
			}
			for (int i = 0; i < samples.Count; i++)
			{
				samples[i].Update();
			}
		}

		private void EndSample(SampleSustainer samp)
		{
			samples.Remove(samp);
			samp.SampleCleanup();
		}

		public virtual void Cleanup()
		{
			while (samples.Count > 0)
			{
				EndSample(samples[0]);
			}
		}

		public override string ToString()
		{
			return subDef.name + "_" + creationFrame;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(creationRealTime.GetHashCode(), subDef);
		}

		public string SamplesDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SampleSustainer sample in samples)
			{
				stringBuilder.AppendLine(sample.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
