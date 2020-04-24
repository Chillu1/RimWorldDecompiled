using System;
using UnityEngine;

namespace Verse.Sound
{
	public class SoundParamTarget_PropertyHighPass : SoundParamTarget
	{
		private HighPassFilterProperty filterProperty;

		public override string Label => "HighPassFilter-" + filterProperty;

		public override Type NeededFilterType => typeof(SoundFilterHighPass);

		public override void SetOn(Sample sample, float value)
		{
			AudioHighPassFilter audioHighPassFilter = sample.source.GetComponent<AudioHighPassFilter>();
			if (audioHighPassFilter == null)
			{
				audioHighPassFilter = sample.source.gameObject.AddComponent<AudioHighPassFilter>();
			}
			if (filterProperty == HighPassFilterProperty.Cutoff)
			{
				audioHighPassFilter.cutoffFrequency = value;
			}
			if (filterProperty == HighPassFilterProperty.Resonance)
			{
				audioHighPassFilter.highpassResonanceQ = value;
			}
		}
	}
}
